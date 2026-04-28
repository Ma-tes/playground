using Microsoft.AspNetCore.Mvc;
using ShareCar.BackendService.Domain.Models;
using ShareCar.BackendService.Domain.Repositories;
using ShareCar.BackendService.Domain.Services;

namespace ShareCar.BackendService.App.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
  private readonly IAuthenticationService _authenticationService;
  private readonly IUserRepository _userRepository;

  public AuthController(IAuthenticationService authenticationService, IUserRepository userRepository)
  {
    _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
    _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
  }

  [HttpPost("login")]
  public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request)
  {
    var token = await _authenticationService.AuthenticateAsync(request.Username, request.Password);
    if (token is null)
    {
      return Unauthorized(new { Message = "Invalid username or password." });
    }

    return Ok(new LoginResponse(token));
  }

  [HttpPost("register")]
  public async Task<ActionResult> RegisterAsync([FromBody] RegisterRequest request)
  {
    var existing = await _userRepository.GetByUsernameAsync(request.Username);
    if (existing is not null)
    {
      return Conflict(new { Message = "A user with this username already exists." });
    }

    var user = new User(
      request.Username,
      PasswordHasher.HashPassword(request.Password),
      request.Email,
      UserRole.User);

    var id = await _userRepository.CreateAsync(user);

    return Created($"api/users/{id}", new { Id = id, user.Username, user.Email });
  }
}

public sealed record LoginRequest(string Username, string Password);
public sealed record LoginResponse(string Token);
public sealed record RegisterRequest(string Username, string Password, string Email);
