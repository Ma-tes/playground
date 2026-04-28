using Microsoft.EntityFrameworkCore;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories;

public class ShareCarDbContext : DbContext
{
  public DbSet<User> Users => Set<User>();
  public DbSet<Vehicle> Vehicles => Set<Vehicle>();
  public DbSet<ParkingLot> ParkingLots => Set<ParkingLot>();
  public DbSet<Booking> Bookings => Set<Booking>();
  public DbSet<BlockLog> BlockLogs => Set<BlockLog>();
  public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();

  public ShareCarDbContext(DbContextOptions<ShareCarDbContext> options) : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShareCarDbContext).Assembly);
  }
}
