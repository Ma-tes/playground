namespace ShareCar.BackendService.Domain.Services;

public interface IAuthenticationService
{
  Task<string?> AuthenticateAsync(string username, string password);
}
