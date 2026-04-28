using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

public interface IUserRepository
{
  Task<User?> GetByIdAsync(int id);
  Task<User?> GetByUsernameAsync(string username);
  Task<IEnumerable<User>> GetAllAsync();
  Task<int> CreateAsync(User user);
  Task UpdateAsync(User user);
  Task DeleteAsync(int id);
}
