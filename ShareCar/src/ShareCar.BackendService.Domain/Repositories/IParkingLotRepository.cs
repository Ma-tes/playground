using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

public interface IParkingLotRepository
{
  Task<ParkingLot?> GetByIdAsync(int id);
  Task<IEnumerable<ParkingLot>> GetAllAsync();
  Task<int> CreateAsync(ParkingLot parkingLot);
  Task UpdateAsync(ParkingLot parkingLot);
  Task AdminUpdateAsync(int id, string name, double latitude, double longitude, int totalCapacity);
  Task DeleteAsync(int id);
}
