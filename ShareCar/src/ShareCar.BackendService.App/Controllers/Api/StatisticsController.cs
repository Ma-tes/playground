using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareCar.BackendService.Domain.Repositories;
using ShareCar.BackendService.Domain.Services;

namespace ShareCar.BackendService.App.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class StatisticsController : ControllerBase
{
  private readonly IShareCarService _shareCarService;
  private readonly IBookingRepository _bookingRepository;

  public StatisticsController(IShareCarService shareCarService, IBookingRepository bookingRepository)
  {
    _shareCarService = shareCarService ?? throw new ArgumentNullException(nameof(shareCarService));
    _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
  }

  [HttpGet("vehicle/{vehicleId}")]
  public async Task<IActionResult> GetVehicleStatisticsAsync(int vehicleId)
  {
    try
    {
      var stats = await _shareCarService.GetVehicleStatisticsAsync(vehicleId);

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

  [HttpGet("overview")]
  public async Task<IActionResult> GetOverviewAsync()
  {
    var bookings = await _bookingRepository.GetAllAsync();
    var bookingList = bookings.ToList();

    return Ok(new
    {
      TotalBookings = bookingList.Count,
      ActiveBookings = bookingList.Count(b => b.IsActive),
      CompletedBookings = bookingList.Count(b => !b.IsActive),
      TotalRevenue = bookingList.Where(b => b.TotalPrice.HasValue).Sum(b => b.TotalPrice!.Value)
    });
  }
}
