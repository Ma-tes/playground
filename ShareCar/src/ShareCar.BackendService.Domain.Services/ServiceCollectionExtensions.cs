using Microsoft.Extensions.DependencyInjection;

namespace ShareCar.BackendService.Domain.Services;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDomainServices(this IServiceCollection services)
  {
    services.AddScoped<IShareCarService, ShareCarService>();
    services.AddScoped<IAuthenticationService, AuthenticationService>();

    return services;
  }
}
