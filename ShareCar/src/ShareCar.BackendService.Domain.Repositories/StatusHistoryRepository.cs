using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

internal sealed class StatusHistoryRepository : IStatusHistoryRepository
{
  private readonly ILogger<StatusHistoryRepository> _logger;
  private readonly ShareCarDbContext _dbContext;

  public StatusHistoryRepository(ILogger<StatusHistoryRepository> logger, ShareCarDbContext dbContext)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
  }

  public async Task<IEnumerable<StatusHistory>> GetByVehicleIdAsync(int vehicleId)
  {
    return await _dbContext.StatusHistories
      .Where(s => s.VehicleId == vehicleId)
      .ToListAsync();
  }

  public async Task<int> CreateAsync(StatusHistory statusHistory)
  {
    _dbContext.StatusHistories.Add(statusHistory);
    await _dbContext.SaveChangesAsync();

    return statusHistory.Id;
  }
}
