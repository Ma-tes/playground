using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareCar.WebClient.App.Models;
using ShareCar.WebClient.App.Services;

namespace ShareCar.WebClient.App.Controllers;

[Authorize]
public class VehiclesController : Controller
{
  private readonly ILogger<VehiclesController> _logger;
  private readonly BackendApiClient _apiClient;

  public VehiclesController(ILogger<VehiclesController> logger, BackendApiClient apiClient)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
  }

  public async Task<IActionResult> Detail(int id, int? parkingLotId)
  {
    var vehicle = await _apiClient.GetVehicleByIdAsync(id);
    if (vehicle is null)
    {
      return NotFound();
    }

    var stats = await _apiClient.GetVehicleStatisticsAsync(id);
    var activeBooking = await _apiClient.GetActiveBookingAsync();

    var model = new VehicleDetailViewModel
    {
      Vehicle = vehicle,
      Statistics = stats,
      ParkingLotId = parkingLotId,
      ActiveBooking = activeBooking
    };

    return PartialView("_VehicleDetail", model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Rent(int vehicleId, int? parkingLotId)
  {
    var result = await _apiClient.RentVehicleAsync(vehicleId);
    if (result is null)
    {
      TempData["Error"] = _apiClient.LastErrorMessage ?? "Failed to rent vehicle.";

      return RedirectToAction("Index", "Dashboard", new { parkingLotId });
    }

    TempData["Success"] = $"Vehicle rented successfully! Booking #{result.Id}";

    return RedirectToAction("Index", "Dashboard", new { parkingLotId });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Return(int bookingId, int parkingLotId, int endOdometer, int vehicleId)
  {
    var result = await _apiClient.ReturnVehicleAsync(bookingId, parkingLotId, endOdometer);
    if (result is null)
    {
      TempData["Error"] = _apiClient.LastErrorMessage ?? "Failed to return vehicle.";

      return RedirectToAction("Index", "Dashboard", new { parkingLotId });
    }

    TempData["Success"] = $"Vehicle returned! Trip cost: {result.TotalPrice:C2}";

    return RedirectToAction("Index", "Dashboard", new { parkingLotId });
  }
}