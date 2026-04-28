using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Services;

public interface IShareCarService
{
  Task<Booking> RentVehicleAsync(int userId, int vehicleId);
  Task<Booking> ReturnVehicleAsync(int bookingId, int returnParkingLotId, int endOdometer);
  Task BlockVehicleAsync(int vehicleId, int adminId, string reason);
  Task UnblockVehicleAsync(int vehicleId);
  Task<decimal> CalculateTripPriceAsync(int startOdometer, int endOdometer, DateTime startTime, DateTime endTime);
  Task<VehicleStatistics> GetVehicleStatisticsAsync(int vehicleId);
}
