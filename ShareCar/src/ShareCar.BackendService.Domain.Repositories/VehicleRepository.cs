using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

internal sealed class VehicleRepository : IVehicleRepository
{
  private readonly ILogger<VehicleRepository> _logger;
  private readonly ShareCarDbContext _dbContext;

  public VehicleRepository(ILogger<VehicleRepository> logger, ShareCarDbContext dbContext)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
  }

  public async Task<Vehicle?> GetByIdAsync(int id)
  {
    return await _dbContext.Vehicles.FindAsync(id);
  }

  public async Task<IEnumerable<Vehicle>> GetAllAsync()
  {
    return await _dbContext.Vehicles.ToListAsync();
  }

  public async Task<IEnumerable<Vehicle>> GetAvailableByParkingLotAsync(int lotId)
  {
    return await _dbContext.Vehicles
      .Where(v => v.CurrentParkingLotId == lotId && v.Status == VehicleStatus.Available)
      .ToListAsync();
  }

  public async Task<int> CreateAsync(Vehicle vehicle)
  {
    _dbContext.Vehicles.Add(vehicle);
    await _dbContext.SaveChangesAsync();

    return vehicle.Id;
  }

  public async Task UpdateAsync(Vehicle vehicle)
  {
    _dbContext.Vehicles.Update(vehicle);
    await _dbContext.SaveChangesAsync();
  }

  public async Task DeleteAsync(int id)
  {
    var vehicle = await _dbContext.Vehicles.FindAsync(id);
    if (vehicle is not null)
    {
      _dbContext.Vehicles.Remove(vehicle);
      await _dbContext.SaveChangesAsync();
    }
  }
}
