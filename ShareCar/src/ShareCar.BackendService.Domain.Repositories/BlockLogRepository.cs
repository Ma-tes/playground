using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

internal sealed class BlockLogRepository : IBlockLogRepository
{
  private readonly ILogger<BlockLogRepository> _logger;
  private readonly ShareCarDbContext _dbContext;

  public BlockLogRepository(ILogger<BlockLogRepository> logger, ShareCarDbContext dbContext)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
  }

  public async Task<BlockLog?> GetByIdAsync(int id)
  {
    return await _dbContext.BlockLogs.FindAsync(id);
  }

  public async Task<IEnumerable<BlockLog>> GetByVehicleIdAsync(int vehicleId)
  {
    return await _dbContext.BlockLogs
      .Where(b => b.VehicleId == vehicleId)
      .ToListAsync();
  }

  public async Task<int> CreateAsync(BlockLog blockLog)
  {
    _dbContext.BlockLogs.Add(blockLog);
    await _dbContext.SaveChangesAsync();

    return blockLog.Id;
  }

  public async Task UpdateAsync(BlockLog blockLog)
  {
    _dbContext.BlockLogs.Update(blockLog);
    await _dbContext.SaveChangesAsync();
  }
}
