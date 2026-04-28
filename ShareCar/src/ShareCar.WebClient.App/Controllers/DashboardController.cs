using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareCar.WebClient.App.Models;
using ShareCar.WebClient.App.Services;

namespace ShareCar.WebClient.App.Controllers;

[Authorize]
public class DashboardController : Controller
{
  private readonly ILogger<DashboardController> _logger;
  private readonly BackendApiClient _apiClient;

  public DashboardController(ILogger<DashboardController> logger, BackendApiClient apiClient)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
  }

  public async Task<IActionResult> Index(int? parkingLotId = null)
  {
    var parkingLots = await _apiClient.GetParkingLotsAsync();
    var activeBooking = await _apiClient.GetActiveBookingAsync();

    VehicleDetailItem? activeVehicle = null;
    if (activeBooking is not null)
    {
      activeVehicle = await _apiClient.GetVehicleByIdAsync(activeBooking.VehicleId);
    }

    var selected = parkingLotId.HasValue
      ? parkingLots.FirstOrDefault(p => p.Id == parkingLotId.Value)
      : null;

    var vehicles = selected is not null
      ? await _apiClient.GetVehiclesByParkingLotAsync(selected.Id)
      : [];

    var model = new DashboardViewModel
    {
      ParkingLots = parkingLots,
      SelectedParkingLotId = parkingLotId,
      SelectedParkingLot = selected,
      Vehicles = vehicles,
      ActiveBooking = activeBooking,
      ActiveVehicle = activeVehicle
    };

    return View(model);
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  [AllowAnonymous]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}
