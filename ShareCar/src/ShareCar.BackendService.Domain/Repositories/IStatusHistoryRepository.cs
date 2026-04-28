using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

public interface IStatusHistoryRepository
{
  Task<IEnumerable<StatusHistory>> GetByVehicleIdAsync(int vehicleId);
  Task<int> CreateAsync(StatusHistory statusHistory);
}
