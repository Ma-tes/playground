using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareCar.BackendService.Domain.Models;
using ShareCar.BackendService.Domain.Repositories;

namespace ShareCar.BackendService.App.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ParkingLotsController : ControllerBase
{
    private readonly IParkingLotRepository _parkingLotRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public ParkingLotsController(IParkingLotRepository parkingLotRepository, IVehicleRepository vehicleRepository)
    {
        _parkingLotRepository = parkingLotRepository ?? throw new ArgumentNullException(nameof(parkingLotRepository));
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var lots = await _parkingLotRepository.GetAllAsync();
        var allVehicles = await _vehicleRepository.GetAllAsync();

        var vehicleCounts = allVehicles
            .Where(v => v.CurrentParkingLotId.HasValue && v.Status != Domain.Models.VehicleStatus.Blocked)
            .GroupBy(v => v.CurrentParkingLotId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = lots.Select(l => new
        {
            l.Id,
            l.Name,
            l.Latitude,
            l.Longitude,
            l.TotalCapacity,
            AvailableVehicles = vehicleCounts.GetValueOrDefault(l.Id, 0)
        });

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateParkingLotRequest request)
    {
        var lot = new ParkingLot(request.Name, request.Latitude, request.Longitude, request.TotalCapacity);
        var id = await _parkingLotRepository.CreateAsync(lot);
        return CreatedAtAction(nameof(GetAllAsync), new { id }, new { Id = id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateParkingLotRequest request)
    {
        var existing = await _parkingLotRepository.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _parkingLotRepository.AdminUpdateAsync(id, request.Name, request.Latitude, request.Longitude, request.TotalCapacity);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var existing = await _parkingLotRepository.GetByIdAsync(id);
        if (existing is null) return NotFound();

        await _parkingLotRepository.DeleteAsync(id);
        return NoContent();
    }
}

public sealed record CreateParkingLotRequest(string Name, double Latitude, double Longitude, int TotalCapacity);
public sealed record UpdateParkingLotRequest(string Name, double Latitude, double Longitude, int TotalCapacity);
