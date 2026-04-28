namespace ShareCar.BackendService.Domain.Models;

public class ParkingLot
{
  public int Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public double Latitude { get; init; }
  public double Longitude { get; init; }
  public int TotalCapacity { get; init; }

  private ParkingLot() { }
}
