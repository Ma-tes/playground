using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

internal sealed class BookingRepository : IBookingRepository
{
  private readonly ILogger<BookingRepository> _logger;
  private readonly ShareCarDbContext _dbContext;

  public BookingRepository(ILogger<BookingRepository> logger, ShareCarDbContext dbContext)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
  }

  public async Task<Booking?> GetByIdAsync(int id)
  {
    return await _dbContext.Bookings.FindAsync(id);
  }

  public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId)
  {
    return await _dbContext.Bookings
      .Where(b => b.UserId == userId)
      .ToListAsync();
  }

  public async Task<Booking?> GetActiveByUserIdAsync(int userId)
  {
    return await _dbContext.Bookings
      .FirstOrDefaultAsync(b => b.UserId == userId && b.IsActive);
  }

  public async Task<IEnumerable<Booking>> GetAllAsync()
  {
    return await _dbContext.Bookings.ToListAsync();
  }

  public async Task<int> CreateAsync(Booking booking)
  {
    _dbContext.Bookings.Add(booking);
    await _dbContext.SaveChangesAsync();

    return booking.Id;
  }

  public async Task UpdateAsync(Booking booking)
  {
    _dbContext.Bookings.Update(booking);
    await _dbContext.SaveChangesAsync();
  }
}
