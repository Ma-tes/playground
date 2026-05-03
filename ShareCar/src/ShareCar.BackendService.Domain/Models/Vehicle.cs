namespace ShareCar.BackendService.Domain.Models;

public class Vehicle
{
  public int Id { get; init; }
  public string Model { get; init; } = string.Empty;
  public string PlateNumber { get; init; } = string.Empty;
  public int? CurrentParkingLotId { get; set; }
  public VehicleStatus Status { get; set; }
  public int Odometer { get; set; }

  public Vehicle(string model, string plateNumber, int? currentParkingLotId, VehicleStatus status, int odometer)
  {
    Model = model;
    PlateNumber = plateNumber;
    CurrentParkingLotId = currentParkingLotId;
    Status = status;
    Odometer = odometer;
  }

  private Vehicle() { }
}
