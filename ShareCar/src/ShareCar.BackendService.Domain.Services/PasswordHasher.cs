using System.Security.Cryptography;

namespace ShareCar.BackendService.Domain.Services;

public static class PasswordHasher
{
  public static string HashPassword(string password)
  {
    var salt = RandomNumberGenerator.GetBytes(16);

    using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
    var hash = pbkdf2.GetBytes(32);

    return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
  }
}
