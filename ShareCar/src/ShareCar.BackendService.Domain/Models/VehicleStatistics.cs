namespace ShareCar.BackendService.Domain.Models;

public class VehicleStatistics
{
  public int TotalTrips { get; init; }
  public int TotalDistanceKm { get; init; }

  public VehicleStatistics(int totalTrips, int totalDistanceKm)
  {
    TotalTrips = totalTrips;
    TotalDistanceKm = totalDistanceKm;
  }

  private VehicleStatistics() { }
}
