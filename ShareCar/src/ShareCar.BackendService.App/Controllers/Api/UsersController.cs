using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareCar.BackendService.Domain.Models;
using ShareCar.BackendService.Domain.Repositories;

namespace ShareCar.BackendService.App.Controllers.Api;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
  private readonly IUserRepository _userRepository;

  public UsersController(IUserRepository userRepository)
  {
    _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
  }

  [HttpGet]
  public async Task<IActionResult> GetAllAsync()
  {
    var users = await _userRepository.GetAllAsync();

    var result = users.Select(u => new
    {
      u.Id,
      u.Username,
      u.Email,
      Role = u.Role.ToString()
    });

    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetByIdAsync(int id)
  {
    var user = await _userRepository.GetByIdAsync(id);
    if (user is null)
    {
      return NotFound();
    }

    return Ok(new
    {
      user.Id,
      user.Username,
      user.Email,
      Role = user.Role.ToString()
    });
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateUserRequest request)
  {
    if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
    {
      return BadRequest(new { Message = $"Invalid role '{request.Role}'. Valid values: User, Admin." });
    }

    var existing = await _userRepository.GetByIdAsync(id);
    if (existing is null) return NotFound();

    await _userRepository.AdminUpdateAsync(id, request.Email, role);
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteAsync(int id)
  {
    var existing = await _userRepository.GetByIdAsync(id);
    if (existing is null) return NotFound();

    await _userRepository.DeleteAsync(id);
    return NoContent();
  }
}

public sealed record UpdateUserRequest(string Email, string Role);
