namespace ShareCar.BackendService.Domain.Models;

public class Booking
{
  public int Id { get; init; }
  public int UserId { get; init; }
  public int VehicleId { get; init; }
  public int StartParkingLotId { get; init; }
  public DateTime StartTime { get; init; }
  public DateTime? EndTime { get; set; }
  public int StartOdometer { get; init; }
  public int? EndOdometer { get; set; }
  public decimal? TotalPrice { get; set; }
  public bool IsActive { get; set; }

  public Booking(int userId, int vehicleId, int startParkingLotId, int startOdometer)
  {
    UserId = userId;
    VehicleId = vehicleId;
    StartParkingLotId = startParkingLotId;
    StartTime = DateTime.UtcNow;
    StartOdometer = startOdometer;
    IsActive = true;
  }

  private Booking() { }
}
