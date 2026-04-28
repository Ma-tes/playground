namespace ShareCar.BackendService.Domain.Models;

public class StatusHistory
{
  public int Id { get; init; }
  public int VehicleId { get; init; }
  public VehicleStatus OldStatus { get; init; }
  public VehicleStatus NewStatus { get; init; }
  public DateTime ChangedAt { get; init; }
  public string TriggeredBy { get; init; } = string.Empty;

  public StatusHistory(int vehicleId, VehicleStatus oldStatus, VehicleStatus newStatus, string triggeredBy)
  {
    VehicleId = vehicleId;
    OldStatus = oldStatus;
    NewStatus = newStatus;
    ChangedAt = DateTime.UtcNow;
    TriggeredBy = triggeredBy;
  }

  private StatusHistory() { }
}
