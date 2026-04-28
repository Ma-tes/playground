using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareCar.WebClient.App.Models;
using ShareCar.WebClient.App.Services;

namespace ShareCar.WebClient.App.Controllers;

[Authorize]
public class ProfileController : Controller
{
  private readonly BackendApiClient _apiClient;

  public ProfileController(BackendApiClient apiClient)
  {
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
  }

  public async Task<IActionResult> Index()
  {
    var activeBooking = await _apiClient.GetActiveBookingAsync();
    var bookings = await _apiClient.GetMyBookingsAsync();

    VehicleDetailItem? activeVehicle = null;
    if (activeBooking is not null)
    {
      activeVehicle = await _apiClient.GetVehicleByIdAsync(activeBooking.VehicleId);
    }

    var model = new ProfileViewModel
    {
      Username = User.Identity?.Name ?? "Unknown",
      ActiveBooking = activeBooking,
      ActiveVehicle = activeVehicle,
      BookingHistory = bookings.Where(b => !b.IsActive).OrderByDescending(b => b.StartTime).ToList()
    };

    return View(model);
  }
}
