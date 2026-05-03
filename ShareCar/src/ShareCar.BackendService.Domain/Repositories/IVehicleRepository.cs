using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

public interface IVehicleRepository
{
  Task<Vehicle?> GetByIdAsync(int id);
  Task<IEnumerable<Vehicle>> GetAllAsync();
  Task<IEnumerable<Vehicle>> GetAvailableByParkingLotAsync(int lotId);
  Task<IEnumerable<Vehicle>> GetFilteredByParkingLotAsync(int lotId, string? search, string? statusFilter, string sortBy, string sortDir);
  Task<IEnumerable<Vehicle>> GetFilteredAcrossLotsAsync(int[] lotIds, string? search, string? statusFilter, string sortBy, string sortDir);
  Task<int> CreateAsync(Vehicle vehicle);
  Task UpdateAsync(Vehicle vehicle);
  Task AdminUpdateAsync(int id, string model, string plateNumber, int odometer, int? currentParkingLotId);
  Task DeleteAsync(int id);
}
