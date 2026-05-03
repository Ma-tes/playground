using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

public interface IBookingRepository
{
  Task<Booking?> GetByIdAsync(int id);
  Task<IEnumerable<Booking>> GetByUserIdAsync(int userId);
  Task<Booking?> GetActiveByUserIdAsync(int userId);
  Task<IEnumerable<Booking>> GetActiveByVehicleIdAsync(int vehicleId);
  Task<bool> HasOverlappingBookingAsync(int vehicleId, DateTime startTime, DateTime endTime);
  Task<bool> HasOverlappingBookingForUserAsync(int userId, DateTime startTime, DateTime endTime);
  Task<IEnumerable<Booking>> GetAllAsync();
  Task<int> CreateAsync(Booking booking);
  Task UpdateAsync(Booking booking);
}
