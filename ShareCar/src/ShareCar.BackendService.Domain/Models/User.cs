namespace ShareCar.BackendService.Domain.Models;

public class User
{
  public int Id { get; init; }
  public string Username { get; init; } = string.Empty;
  public string PasswordHash { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public UserRole Role { get; init; }

  public User(string username, string passwordHash, string email, UserRole role)
  {
    Username = username;
    PasswordHash = passwordHash;
    Email = email;
    Role = role;
  }

  private User() { }
}
