using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

public interface IBlockLogRepository
{
  Task<BlockLog?> GetByIdAsync(int id);
  Task<IEnumerable<BlockLog>> GetByVehicleIdAsync(int vehicleId);
  Task<int> CreateAsync(BlockLog blockLog);
  Task UpdateAsync(BlockLog blockLog);
}
