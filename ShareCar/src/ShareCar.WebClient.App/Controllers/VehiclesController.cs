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
    var blockedRanges = await _apiClient.GetVehicleBookingsAsync(id);

    var model = new VehicleDetailViewModel
    {
      Vehicle = vehicle,
      Statistics = stats,
      ParkingLotId = parkingLotId,
      ActiveBooking = activeBooking,
      BlockedRanges = blockedRanges
    };

    return PartialView("_VehicleDetail", model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Rent(int vehicleId, int? parkingLotId, DateTime startTime, DateTime endTime)
  {
    var result = await _apiClient.RentVehicleAsync(vehicleId, startTime, endTime);
    if (result is null)
    {
      return Json(new { success = false, message = _apiClient.LastErrorMessage ?? "Failed to rent vehicle." });
    }

    return Json(new { success = true, message = $"Vehicle rented successfully! Booking #{result.Id}" });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Return(int bookingId, int parkingLotId, int endOdometer, int vehicleId)
  {
    var result = await _apiClient.ReturnVehicleAsync(bookingId, parkingLotId, endOdometer);

    if (result is null)
    {
      return Json(new { success = false, message = _apiClient.LastErrorMessage ?? "Failed to return vehicle." });
    }

    return Json(new { success = true, message = $"Vehicle returned! Trip cost: {result.TotalPrice:C2}" });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Cancel(int bookingId)
  {
    var ok = await _apiClient.CancelBookingAsync(bookingId);

    if (!ok)
    {
      return Json(new { success = false, message = _apiClient.LastErrorMessage ?? "Failed to cancel booking." });
    }

    return Json(new { success = true, message = "Booking cancelled successfully." });
  }
}
