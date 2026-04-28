using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareCar.BackendService.Domain.Repositories;
using ShareCar.BackendService.Domain.Services;

namespace ShareCar.BackendService.App.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class BookingsController : ControllerBase
{
  private readonly IShareCarService _shareCarService;
  private readonly IBookingRepository _bookingRepository;

  public BookingsController(IShareCarService shareCarService, IBookingRepository bookingRepository)
  {
    _shareCarService = shareCarService ?? throw new ArgumentNullException(nameof(shareCarService));
    _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
  }

  [HttpPost("rent")]
  public async Task<IActionResult> RentAsync([FromBody] RentRequest request)
  {
    var userId = GetUserId();

    try
    {
      var booking = await _shareCarService.RentVehicleAsync(userId, request.VehicleId);

      return Ok(new
      {
        booking.Id,
        booking.VehicleId,
        booking.StartParkingLotId,
        booking.StartTime,
        booking.StartOdometer,
        booking.IsActive
      });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { ex.Message });
    }
  }

  [HttpPost("{id}/return")]
  public async Task<IActionResult> ReturnAsync(int id, [FromBody] ReturnRequest request)
  {
    try
    {
      var booking = await _shareCarService.ReturnVehicleAsync(id, request.ParkingLotId, request.EndOdometer);

      return Ok(new
      {
        booking.Id,
        booking.VehicleId,
        booking.StartParkingLotId,
        booking.StartTime,
        booking.EndTime,
        booking.StartOdometer,
        booking.EndOdometer,
        booking.TotalPrice,
        booking.IsActive
      });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { ex.Message });
    }
  }

  [HttpGet("active")]
  public async Task<IActionResult> GetActiveAsync()
  {
    var userId = GetUserId();
    var booking = await _bookingRepository.GetActiveByUserIdAsync(userId);

    if (booking is null)
    {
      return NotFound();
    }

    return Ok(new
    {
      booking.Id,
      booking.VehicleId,
      booking.StartParkingLotId,
      booking.StartTime,
      booking.StartOdometer,
      booking.IsActive
    });
  }

  [HttpGet("my")]
  public async Task<IActionResult> GetMyBookingsAsync()
  {
    var userId = GetUserId();
    var bookings = await _bookingRepository.GetByUserIdAsync(userId);

    var result = bookings.Select(b => new
    {
      b.Id,
      b.VehicleId,
      b.StartParkingLotId,
      b.StartTime,
      b.EndTime,
      b.StartOdometer,
      b.EndOdometer,
      b.TotalPrice,
      b.IsActive
    });

    return Ok(result);
  }

  private int GetUserId()
  {
    var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? throw new InvalidOperationException("User ID claim not found.");

    return int.Parse(claim);
  }
}

public sealed record RentRequest(int VehicleId);
public sealed record ReturnRequest(int ParkingLotId, int EndOdometer);
