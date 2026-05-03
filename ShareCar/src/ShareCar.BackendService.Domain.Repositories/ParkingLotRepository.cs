using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

internal sealed class ParkingLotRepository : IParkingLotRepository
{
  private readonly ILogger<ParkingLotRepository> _logger;
  private readonly ShareCarDbContext _dbContext;

  public ParkingLotRepository(ILogger<ParkingLotRepository> logger, ShareCarDbContext dbContext)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
  }

  public async Task<IEnumerable<ParkingLot>> GetAllAsync()
  {
    return await _dbContext.ParkingLots.ToListAsync();
  }

  public async Task<ParkingLot?> GetByIdAsync(int id)
  {
    return await _dbContext.ParkingLots.FindAsync(id);
  }

  public async Task<int> CreateAsync(ParkingLot parkingLot)
  {
    _dbContext.ParkingLots.Add(parkingLot);
    await _dbContext.SaveChangesAsync();

    return parkingLot.Id;
  }

  public async Task UpdateAsync(ParkingLot parkingLot)
  {
    _dbContext.ParkingLots.Update(parkingLot);
    await _dbContext.SaveChangesAsync();
  }

  public async Task AdminUpdateAsync(int id, string name, double latitude, double longitude, int totalCapacity)
  {
    await _dbContext.ParkingLots
      .Where(p => p.Id == id)
      .ExecuteUpdateAsync(s => s
        .SetProperty(p => p.Name, name)
        .SetProperty(p => p.Latitude, latitude)
        .SetProperty(p => p.Longitude, longitude)
        .SetProperty(p => p.TotalCapacity, totalCapacity));
  }

  public async Task DeleteAsync(int id)
  {
    ParkingLot? parkingLot = await _dbContext.ParkingLots.FindAsync(id);

    if (parkingLot is not null)
    {
      _dbContext.ParkingLots.Remove(parkingLot);
      await _dbContext.SaveChangesAsync();
    }
  }
}
