using ShareCar.BackendService.Domain.Configuration;

namespace ShareCar.BackendService.App.Configuration;

internal sealed class JwtConfiguration : IJwtConfiguration
{
  public string SecretKey { get; set; } = string.Empty;
  public string Issuer { get; set; } = string.Empty;
  public string Audience { get; set; } = string.Empty;
  public int ExpirationMinutes { get; set; }
}
