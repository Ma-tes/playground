namespace ShareCar.BackendService.Domain.Models;

public class BlockLog
{
  public int Id { get; init; }
  public int VehicleId { get; init; }
  public int AdminId { get; init; }
  public DateTime StartTime { get; init; }
  public DateTime? EndTime { get; set; }
  public string Reason { get; init; } = string.Empty;

  public BlockLog(int vehicleId, int adminId, string reason)
  {
    VehicleId = vehicleId;
    AdminId = adminId;
    StartTime = DateTime.UtcNow;
    Reason = reason;
  }

  private BlockLog() { }
}
