using Microsoft.Extensions.Logging;
using ShareCar.BackendService.Domain.Configuration;
using ShareCar.BackendService.Domain.Models;
using ShareCar.BackendService.Domain.Repositories;

namespace ShareCar.BackendService.Domain.Services;

internal sealed class ShareCarService : IShareCarService
{
  private readonly ILogger<ShareCarService> _logger;
  private readonly IVehicleRepository _vehicleRepository;
  private readonly IBookingRepository _bookingRepository;
  private readonly IBlockLogRepository _blockLogRepository;
  private readonly IStatusHistoryRepository _statusHistoryRepository;
  private readonly IPricingConfiguration _pricingConfiguration;

  public ShareCarService(
    ILogger<ShareCarService> logger,
    IVehicleRepository vehicleRepository,
    IBookingRepository bookingRepository,
    IBlockLogRepository blockLogRepository,
    IStatusHistoryRepository statusHistoryRepository,
    IPricingConfiguration pricingConfiguration)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
    _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
    _blockLogRepository = blockLogRepository ?? throw new ArgumentNullException(nameof(blockLogRepository));
    _statusHistoryRepository = statusHistoryRepository ?? throw new ArgumentNullException(nameof(statusHistoryRepository));
    _pricingConfiguration = pricingConfiguration ?? throw new ArgumentNullException(nameof(pricingConfiguration));
  }

  public async Task<Booking> RentVehicleAsync(int userId, int vehicleId)
  {
    var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId)
      ?? throw new InvalidOperationException($"Vehicle {vehicleId} not found.");

    if (vehicle.Status != VehicleStatus.Available)
    {
      throw new InvalidOperationException($"Vehicle {vehicleId} is not available (current status: {vehicle.Status}).");
    }

    if (!vehicle.CurrentParkingLotId.HasValue)
    {
      throw new InvalidOperationException($"Vehicle {vehicleId} is not parked at any lot.");
    }

    var activeBooking = await _bookingRepository.GetActiveByUserIdAsync(userId);
    if (activeBooking is not null)
    {
      throw new InvalidOperationException($"User {userId} already has an active booking ({activeBooking.Id}).");
    }

    var startParkingLotId = vehicle.CurrentParkingLotId.Value;
    var booking = new Booking(userId, vehicleId, startParkingLotId, vehicle.Odometer);
    await _bookingRepository.CreateAsync(booking);

    var oldStatus = vehicle.Status;
    vehicle.Status = VehicleStatus.Rented;
    vehicle.CurrentParkingLotId = null;
    await _vehicleRepository.UpdateAsync(vehicle);

    await _statusHistoryRepository.CreateAsync(
      new StatusHistory(vehicleId, oldStatus, VehicleStatus.Rented, $"User:{userId}"));

    _logger.LogInformation("User {UserId} rented vehicle {VehicleId}, booking {BookingId}", userId, vehicleId, booking.Id);

    return booking;
  }

  public async Task<Booking> ReturnVehicleAsync(int bookingId, int returnParkingLotId, int endOdometer)
  {
    var booking = await _bookingRepository.GetByIdAsync(bookingId)
      ?? throw new InvalidOperationException($"Booking {bookingId} not found.");

    if (!booking.IsActive)
    {
      throw new InvalidOperationException($"Booking {bookingId} is not active.");
    }

    if (returnParkingLotId != booking.StartParkingLotId)
    {
      throw new InvalidOperationException($"Vehicle must be returned to the same parking lot where it was rented (lot {booking.StartParkingLotId}).");
    }

    var vehicle = await _vehicleRepository.GetByIdAsync(booking.VehicleId)
      ?? throw new InvalidOperationException($"Vehicle {booking.VehicleId} not found.");

    var endTime = DateTime.UtcNow;
    var totalPrice = await CalculateTripPriceAsync(booking.StartOdometer, endOdometer, booking.StartTime, endTime);

    booking.EndTime = endTime;
    booking.EndOdometer = endOdometer;
    booking.TotalPrice = totalPrice;
    booking.IsActive = false;
    await _bookingRepository.UpdateAsync(booking);

    var oldStatus = vehicle.Status;
    vehicle.Status = VehicleStatus.Available;
    vehicle.Odometer = endOdometer;
    vehicle.CurrentParkingLotId = returnParkingLotId;
    await _vehicleRepository.UpdateAsync(vehicle);

    await _statusHistoryRepository.CreateAsync(
      new StatusHistory(vehicle.Id, oldStatus, VehicleStatus.Available, $"User:{booking.UserId}"));

    _logger.LogInformation(
      "Booking {BookingId} returned: vehicle {VehicleId}, distance {Distance} km, price {Price:C}",
      bookingId, vehicle.Id, endOdometer - booking.StartOdometer, totalPrice);

    return booking;
  }

  public async Task BlockVehicleAsync(int vehicleId, int adminId, string reason)
  {
    var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId)
      ?? throw new InvalidOperationException($"Vehicle {vehicleId} not found.");

    if (vehicle.Status == VehicleStatus.Blocked)
    {
      throw new InvalidOperationException($"Vehicle {vehicleId} is already blocked.");
    }

    var oldStatus = vehicle.Status;
    vehicle.Status = VehicleStatus.Blocked;
    await _vehicleRepository.UpdateAsync(vehicle);

    await _blockLogRepository.CreateAsync(new BlockLog(vehicleId, adminId, reason));

    await _statusHistoryRepository.CreateAsync(
      new StatusHistory(vehicleId, oldStatus, VehicleStatus.Blocked, $"Admin:{adminId}"));

    _logger.LogInformation("Vehicle {VehicleId} blocked by admin {AdminId}: {Reason}", vehicleId, adminId, reason);
  }

  public async Task UnblockVehicleAsync(int vehicleId)
  {
    var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId)
      ?? throw new InvalidOperationException($"Vehicle {vehicleId} not found.");

    if (vehicle.Status != VehicleStatus.Blocked)
    {
      throw new InvalidOperationException($"Vehicle {vehicleId} is not blocked (current status: {vehicle.Status}).");
    }

    var oldStatus = vehicle.Status;
    vehicle.Status = VehicleStatus.Available;
    await _vehicleRepository.UpdateAsync(vehicle);

    var blockLogs = await _blockLogRepository.GetByVehicleIdAsync(vehicleId);
    var activeLog = blockLogs.FirstOrDefault(b => b.EndTime is null);
    if (activeLog is not null)
    {
      activeLog.EndTime = DateTime.UtcNow;
      await _blockLogRepository.UpdateAsync(activeLog);
    }

    await _statusHistoryRepository.CreateAsync(
      new StatusHistory(vehicleId, oldStatus, VehicleStatus.Available, "Admin:unblock"));

    _logger.LogInformation("Vehicle {VehicleId} unblocked", vehicleId);
  }

  public Task<decimal> CalculateTripPriceAsync(int startOdometer, int endOdometer, DateTime startTime, DateTime endTime)
  {
    int distanceKm = endOdometer - startOdometer;
    decimal hours = (decimal)(endTime - startTime).TotalHours;

    decimal distanceCost = distanceKm * _pricingConfiguration.RatePerKm;
    decimal timeCost = hours * _pricingConfiguration.RatePerHour;

    return Task.FromResult(Math.Round(distanceCost + timeCost, 2));
  }

  public async Task<VehicleStatistics> GetVehicleStatisticsAsync(int vehicleId)
  {
    var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId)
      ?? throw new InvalidOperationException($"Vehicle {vehicleId} not found.");

    var bookings = await _bookingRepository.GetAllAsync();
    var lastMonth = DateTime.UtcNow.AddMonths(-1);
    var vehicleBookings = bookings
      .Where(b => b.VehicleId == vehicleId && !b.IsActive && b.StartTime >= lastMonth)
      .ToList();

    var totalTrips = vehicleBookings.Count;
    var totalDistance = vehicleBookings
      .Where(b => b.EndOdometer.HasValue)
      .Sum(b => b.EndOdometer!.Value - b.StartOdometer);

    return new VehicleStatistics(totalTrips, totalDistance);
  }
}
