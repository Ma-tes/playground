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
  private readonly IBlockLogRepository _blockLogRepository;

  public VehiclesController(IShareCarService shareCarService, IVehicleRepository vehicleRepository, IBlockLogRepository blockLogRepository)
  {
    _shareCarService = shareCarService ?? throw new ArgumentNullException(nameof(shareCarService));
    _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
    _blockLogRepository = blockLogRepository ?? throw new ArgumentNullException(nameof(blockLogRepository));
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

  [HttpGet("by-parking-lot/{parkingLotId}/filter")]
  public async Task<IActionResult> GetByParkingLotFilteredAsync(
    int parkingLotId,
    [FromQuery] string? search = null,
    [FromQuery] string? statusFilter = null,
    [FromQuery] string sortBy = "model",
    [FromQuery] string sortDir = "asc")
  {
    var vehicles = await _vehicleRepository.GetFilteredByParkingLotAsync(parkingLotId, search, statusFilter, sortBy, sortDir);

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

  [HttpGet("search")]
  public async Task<IActionResult> SearchAcrossLotsAsync(
    [FromQuery] int[] parkingLotIds,
    [FromQuery] string? search = null,
    [FromQuery] string? statusFilter = null,
    [FromQuery] string sortBy = "model",
    [FromQuery] string sortDir = "asc")
  {
    if (parkingLotIds is null || parkingLotIds.Length == 0)
    {
      return Ok(Array.Empty<object>());
    }

    var vehicles = await _vehicleRepository.GetFilteredAcrossLotsAsync(parkingLotIds, search, statusFilter, sortBy, sortDir);

    var result = vehicles.Select(v => new
    {
      v.Id,
      v.Model,
      v.PlateNumber,
      v.CurrentParkingLotId,
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

  [Authorize(Roles = "Admin")]
  [HttpGet("{id}/blocks")]
  public async Task<IActionResult> GetBlockHistoryAsync(int id)
  {
    var vehicle = await _vehicleRepository.GetByIdAsync(id);
    if (vehicle is null) return NotFound();

    var logs = await _blockLogRepository.GetByVehicleIdAsync(id);

    var result = logs.OrderByDescending(l => l.StartTime).Select(l => new
    {
      l.Id,
      l.VehicleId,
      l.AdminId,
      l.Reason,
      l.StartTime,
      l.EndTime,
      IsActive = l.EndTime is null
    });

    return Ok(result);
  }

  [Authorize(Roles = "Admin")]
  [HttpPost]
  public async Task<IActionResult> CreateAsync([FromBody] CreateVehicleRequest request)
  {
    var vehicle = new Vehicle(request.Model, request.PlateNumber, request.CurrentParkingLotId, VehicleStatus.Available, request.Odometer);
    var id = await _vehicleRepository.CreateAsync(vehicle);
    return CreatedAtAction(nameof(GetByIdAsync), new { id }, new { Id = id });
  }

  [Authorize(Roles = "Admin")]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateVehicleRequest request)
  {
    var existing = await _vehicleRepository.GetByIdAsync(id);
    if (existing is null) return NotFound();

    await _vehicleRepository.AdminUpdateAsync(id, request.Model, request.PlateNumber, request.Odometer, request.CurrentParkingLotId);
    return NoContent();
  }

  [Authorize(Roles = "Admin")]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteAsync(int id)
  {
    var existing = await _vehicleRepository.GetByIdAsync(id);
    if (existing is null) return NotFound();

    await _vehicleRepository.DeleteAsync(id);
    return NoContent();
  }
}

public sealed record BlockRequest(string Reason);
public sealed record CreateVehicleRequest(string Model, string PlateNumber, int? CurrentParkingLotId, int Odometer);
public sealed record UpdateVehicleRequest(string Model, string PlateNumber, int? CurrentParkingLotId, int Odometer);
