using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

public interface IBookingRepository
{
  Task<Booking?> GetByIdAsync(int id);
  Task<IEnumerable<Booking>> GetByUserIdAsync(int userId);
  Task<Booking?> GetActiveByUserIdAsync(int userId);
  Task<IEnumerable<Booking>> GetAllAsync();
  Task<int> CreateAsync(Booking booking);
  Task UpdateAsync(Booking booking);
}
