using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            .Where(v => v.CurrentParkingLotId.HasValue && v.Status == Domain.Models.VehicleStatus.Available)
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
}
