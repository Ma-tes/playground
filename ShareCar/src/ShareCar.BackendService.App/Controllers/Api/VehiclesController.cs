using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareCar.BackendService.Domain.Models;
using ShareCar.BackendService.Domain.Repositories;
using ShareCar.BackendService.Domain.Services;

namespace ShareCar.BackendService.App.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class VehiclesController : ControllerBase
{
  private readonly IShareCarService _shareCarService;
  private readonly IVehicleRepository _vehicleRepository;

  public VehiclesController(IShareCarService shareCarService, IVehicleRepository vehicleRepository)
  {
    _shareCarService = shareCarService ?? throw new ArgumentNullException(nameof(shareCarService));
    _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
  }

  [HttpGet("by-parking-lot/{parkingLotId}")]
  public async Task<IActionResult> GetByParkingLotAsync(int parkingLotId)
  {
    var vehicles = await _vehicleRepository.GetAvailableByParkingLotAsync(parkingLotId);

    var result = vehicles.Select(v => new
    {
      v.Id,
      v.Model,
      v.PlateNumber,
      Status = v.Status.ToString(),
      v.Odometer
    });

    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetByIdAsync(int id)
  {
    var vehicle = await _vehicleRepository.GetByIdAsync(id);
    if (vehicle is null)
    {
      return NotFound();
    }

    return Ok(new
    {
      vehicle.Id,
      vehicle.Model,
      vehicle.PlateNumber,
      vehicle.CurrentParkingLotId,
      Status = vehicle.Status.ToString(),
      vehicle.Odometer
    });
  }

  [Authorize(Roles = "Admin")]
  [HttpPost("{id}/block")]
  public async Task<IActionResult> BlockAsync(int id, [FromBody] BlockRequest request)
  {
    var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    try
    {
      await _shareCarService.BlockVehicleAsync(id, adminId, request.Reason);

      return Ok(new { Message = $"Vehicle {id} has been blocked." });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { ex.Message });
    }
  }

  [Authorize(Roles = "Admin")]
  [HttpPost("{id}/unblock")]
  public async Task<IActionResult> UnblockAsync(int id)
  {
    try
    {
      await _shareCarService.UnblockVehicleAsync(id);

      return Ok(new { Message = $"Vehicle {id} has been unblocked." });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { ex.Message });
    }
  }

  [HttpGet("{id}/statistics")]
  public async Task<IActionResult> GetStatisticsAsync(int id)
  {
    try
    {
      var stats = await _shareCarService.GetVehicleStatisticsAsync(id);

      return Ok(new
      {
        stats.TotalTrips,
        stats.TotalDistanceKm
      });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { ex.Message });
    }
  }
}

public sealed record BlockRequest(string Reason);
