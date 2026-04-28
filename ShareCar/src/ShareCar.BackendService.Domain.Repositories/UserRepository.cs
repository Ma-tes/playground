using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

internal sealed class UserRepository : IUserRepository
{
  private readonly ILogger<UserRepository> _logger;
  private readonly ShareCarDbContext _dbContext;

  public UserRepository(ILogger<UserRepository> logger, ShareCarDbContext dbContext)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
  }

  public async Task<User?> GetByIdAsync(int id)
  {
    return await _dbContext.Users.FindAsync(id);
  }

  public async Task<User?> GetByUsernameAsync(string username)
  {
    return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
  }

  public async Task<IEnumerable<User>> GetAllAsync()
  {
    return await _dbContext.Users.ToListAsync();
  }

  public async Task<int> CreateAsync(User user)
  {
    _dbContext.Users.Add(user);
    await _dbContext.SaveChangesAsync();

    return user.Id;
  }

  public async Task UpdateAsync(User user)
  {
    _dbContext.Users.Update(user);
    await _dbContext.SaveChangesAsync();
  }

  public async Task DeleteAsync(int id)
  {
    var user = await _dbContext.Users.FindAsync(id);
    if (user is not null)
    {
      _dbContext.Users.Remove(user);
      await _dbContext.SaveChangesAsync();
    }
  }
}
