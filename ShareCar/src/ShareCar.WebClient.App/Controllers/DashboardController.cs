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

  public async Task<IActionResult> Index()
  {
    var parkingLots = await _apiClient.GetParkingLotsAsync();

    var model = new DashboardViewModel
    {
      ParkingLots = parkingLots,
    };

    return View(model);
  }

  public async Task<IActionResult> Search(
    [FromQuery] int[]? parkingLotIds = null,
    string? search = null,
    string? statusFilter = null,
    string sortBy = "model",
    string sortDir = "asc")
  {
    var parkingLots = await _apiClient.GetParkingLotsAsync();

    List<VehicleItem> vehicles;
    int totalVehicles;

    var relativeParkingLotIds = parkingLotIds is { Length: > 0 } ?
      parkingLotIds :
      [.. parkingLots.Select(p => p.Id)];

    var allInParkingLots = await _apiClient.SearchVehiclesAcrossLotsAsync(relativeParkingLotIds, null, null, sortBy, sortDir);
    vehicles = await _apiClient.SearchVehiclesAcrossLotsAsync(relativeParkingLotIds, search, statusFilter, sortBy, sortDir);
    totalVehicles = allInParkingLots.Count;

    var model = new DashboardViewModel
    {
      ParkingLots = parkingLots,
      SelectedParkingLotIds = relativeParkingLotIds,
      Vehicles = vehicles,
      Search = search,
      StatusFilter = statusFilter,
      SortBy = sortBy,
      SortDir = sortDir,
      TotalVehicles = totalVehicles,
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
