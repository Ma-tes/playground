namespace ShareCar.BackendService.Domain.Configuration;

public interface IPricingConfiguration
{
  decimal RatePerKm { get; }
  decimal RatePerHour { get; }
}
