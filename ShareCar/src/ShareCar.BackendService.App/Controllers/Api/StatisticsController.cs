using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareCar.BackendService.Domain.Repositories;
using ShareCar.BackendService.Domain.Services;

namespace ShareCar.BackendService.App.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class StatisticsController : ControllerBase
{
  private readonly IShareCarService _shareCarService;
  private readonly IBookingRepository _bookingRepository;
  private readonly IUserRepository _userRepository;
  private readonly IVehicleRepository _vehicleRepository;
  private readonly IParkingLotRepository _parkingLotRepository;

  public StatisticsController(
    IShareCarService shareCarService,
    IBookingRepository bookingRepository,
    IUserRepository userRepository,
    IVehicleRepository vehicleRepository,
    IParkingLotRepository parkingLotRepository)
  {
    _shareCarService = shareCarService ?? throw new ArgumentNullException(nameof(shareCarService));
    _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
    _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
    _parkingLotRepository = parkingLotRepository ?? throw new ArgumentNullException(nameof(parkingLotRepository));
  }

  [HttpGet("vehicle/{vehicleId}")]
  public async Task<IActionResult> GetVehicleStatisticsAsync(int vehicleId)
  {
    try
    {
      var stats = await _shareCarService.GetVehicleStatisticsAsync(vehicleId);

      return Ok(new
      {
        stats.TotalTrips,
        stats.TotalDistanceKm
      });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { ex.Message });
    }
  }

  [HttpGet("overview")]
  public async Task<IActionResult> GetOverviewAsync()
  {
    var bookings = await _bookingRepository.GetAllAsync();
    var bookingList = bookings.ToList();
    var users = await _userRepository.GetAllAsync();
    var vehicles = await _vehicleRepository.GetAllAsync();
    var lots = await _parkingLotRepository.GetAllAsync();

    return Ok(new
    {
      TotalBookings = bookingList.Count,
      ActiveBookings = bookingList.Count(b => b.IsActive),
      CompletedBookings = bookingList.Count(b => !b.IsActive),
      TotalRevenue = bookingList.Where(b => b.TotalPrice.HasValue).Sum(b => b.TotalPrice!.Value),
      TotalUsers = users.Count(),
      TotalVehicles = vehicles.Count(),
      TotalParkingLots = lots.Count()
    });
  }
}
