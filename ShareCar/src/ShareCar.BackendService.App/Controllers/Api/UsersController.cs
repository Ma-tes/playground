using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
