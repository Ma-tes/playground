using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ShareCar.BackendService.Domain.Repositories;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDomainRepositories(this IServiceCollection services, string connectionString)
  {
    services.AddDbContext<ShareCarDbContext>(options =>
      options.UseSqlite(connectionString));

    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IVehicleRepository, VehicleRepository>();
    services.AddScoped<IParkingLotRepository, ParkingLotRepository>();
    services.AddScoped<IBookingRepository, BookingRepository>();
    services.AddScoped<IBlockLogRepository, BlockLogRepository>();
    services.AddScoped<IStatusHistoryRepository, StatusHistoryRepository>();

    return services;
  }
}
