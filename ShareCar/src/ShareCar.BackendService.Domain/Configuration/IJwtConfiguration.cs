namespace ShareCar.BackendService.Domain.Configuration;

public interface IJwtConfiguration
{
  string SecretKey { get; }
  string Issuer { get; }
  string Audience { get; }
  int ExpirationMinutes { get; }
}
