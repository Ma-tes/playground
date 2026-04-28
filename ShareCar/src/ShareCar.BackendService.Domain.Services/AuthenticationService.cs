using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ShareCar.BackendService.Domain.Configuration;
using ShareCar.BackendService.Domain.Repositories;

namespace ShareCar.BackendService.Domain.Services;

internal sealed class AuthenticationService : IAuthenticationService
{
  private readonly ILogger<AuthenticationService> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IJwtConfiguration _jwtConfiguration;

  public AuthenticationService(
    ILogger<AuthenticationService> logger,
    IUserRepository userRepository,
    IJwtConfiguration jwtConfiguration)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    _jwtConfiguration = jwtConfiguration ?? throw new ArgumentNullException(nameof(jwtConfiguration));
  }

  public async Task<string?> AuthenticateAsync(string username, string password)
  {
    var user = await _userRepository.GetByUsernameAsync(username);
    if (user is null)
    {
      _logger.LogWarning("Authentication failed: user '{Username}' not found", username);

      return null;
    }

    if (!VerifyPassword(password, user.PasswordHash))
    {
      _logger.LogWarning("Authentication failed: invalid password for user '{Username}'", username);

      return null;
    }

    var token = GenerateJwtToken(user.Id, user.Username, user.Role.ToString());
    _logger.LogInformation("User '{Username}' authenticated successfully", username);

    return token;
  }

  private string GenerateJwtToken(int userId, string username, string role)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
      new Claim(ClaimTypes.Name, username),
      new Claim(ClaimTypes.Role, role)
    };

    var token = new JwtSecurityToken(
      issuer: _jwtConfiguration.Issuer,
      audience: _jwtConfiguration.Audience,
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_jwtConfiguration.ExpirationMinutes),
      signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  private static bool VerifyPassword(string password, string storedHash)
  {
    var parts = storedHash.Split(':');
    if (parts.Length != 2)
    {
      return false;
    }

    var salt = Convert.FromBase64String(parts[0]);
    var hash = Convert.FromBase64String(parts[1]);

    using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
    var computedHash = pbkdf2.GetBytes(32);

    return CryptographicOperations.FixedTimeEquals(computedHash, hash);
  }
}
