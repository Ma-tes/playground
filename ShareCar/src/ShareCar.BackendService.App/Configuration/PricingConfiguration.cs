using ShareCar.BackendService.Domain.Configuration;

namespace ShareCar.BackendService.App.Configuration;

internal sealed class PricingConfiguration : IPricingConfiguration
{
  public decimal RatePerKm { get; set; }
  public decimal RatePerHour { get; set; }
}
