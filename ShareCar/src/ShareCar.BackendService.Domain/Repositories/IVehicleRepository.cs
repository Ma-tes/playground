using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

public interface IVehicleRepository
{
  Task<Vehicle?> GetByIdAsync(int id);
  Task<IEnumerable<Vehicle>> GetAllAsync();
  Task<IEnumerable<Vehicle>> GetAvailableByParkingLotAsync(int lotId);
  Task<int> CreateAsync(Vehicle vehicle);
  Task UpdateAsync(Vehicle vehicle);
  Task DeleteAsync(int id);
}
