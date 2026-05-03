namespace ShareCar.WebClient.App.Models;

public class DashboardViewModel
{
  public List<ParkingLotItem> ParkingLots { get; set; } = [];
  public int? SelectedParkingLotId { get; set; }
  public int[] SelectedParkingLotIds { get; set; } = [];
  public ParkingLotItem? SelectedParkingLot { get; set; }
  public List<VehicleItem> Vehicles { get; set; } = [];
  public ActiveBookingItem? ActiveBooking { get; set; }
  public VehicleDetailItem? ActiveVehicle { get; set; }

  public string? Search { get; set; }
  public string? StatusFilter { get; set; }
  public string SortBy { get; set; } = "model";
  public string SortDir { get; set; } = "asc";
  public int TotalVehicles { get; set; }
  public bool IsSearchMode => SelectedParkingLotIds.Length > 0;
}

public class ParkingLotItem
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public double Latitude { get; set; }
  public double Longitude { get; set; }
  public int TotalCapacity { get; set; }
  public int AvailableVehicles { get; set; }
}

public class VehicleItem
{
  public int Id { get; set; }
  public string Model { get; set; } = string.Empty;
  public string PlateNumber { get; set; } = string.Empty;
  public int? CurrentParkingLotId { get; set; }
  public string Status { get; set; } = string.Empty;
  public int Odometer { get; set; }
}

public class VehicleDetailItem
{
  public int Id { get; set; }
  public string Model { get; set; } = string.Empty;
  public string PlateNumber { get; set; } = string.Empty;
  public int? CurrentParkingLotId { get; set; }
  public string Status { get; set; } = string.Empty;
  public int Odometer { get; set; }
}

public class ActiveBookingItem
{
  public int Id { get; set; }
  public int VehicleId { get; set; }
  public int StartParkingLotId { get; set; }
  public DateTime StartTime { get; set; }
  public int StartOdometer { get; set; }
  public bool IsActive { get; set; }
}

public class RentResultItem
{
  public int Id { get; set; }
  public int VehicleId { get; set; }
  public int StartParkingLotId { get; set; }
  public DateTime StartTime { get; set; }
  public int StartOdometer { get; set; }
  public bool IsActive { get; set; }
}

public class ReturnResultItem
{
  public int Id { get; set; }
  public int VehicleId { get; set; }
  public int StartParkingLotId { get; set; }
  public DateTime StartTime { get; set; }
  public DateTime? EndTime { get; set; }
  public int StartOdometer { get; set; }
  public int? EndOdometer { get; set; }
  public decimal? TotalPrice { get; set; }
  public bool IsActive { get; set; }
}

public class BookingHistoryItem
{
  public int Id { get; set; }
  public int VehicleId { get; set; }
  public int StartParkingLotId { get; set; }
  public DateTime StartTime { get; set; }
  public DateTime? EndTime { get; set; }
  public int StartOdometer { get; set; }
  public int? EndOdometer { get; set; }
  public decimal? TotalPrice { get; set; }
  public bool IsActive { get; set; }
}

public class VehicleStatisticsItem
{
  public int TotalTrips { get; set; }
  public int TotalDistanceKm { get; set; }
}

public class VehicleDetailViewModel
{
  public VehicleDetailItem Vehicle { get; set; } = null!;
  public VehicleStatisticsItem? Statistics { get; set; }
  public int? ParkingLotId { get; set; }
  public ActiveBookingItem? ActiveBooking { get; set; }
  public List<BookingRangeItem> BlockedRanges { get; set; } = [];
}

public class ProfileViewModel
{
  public string Username { get; set; } = string.Empty;
  public ActiveBookingItem? ActiveBooking { get; set; }
  public VehicleDetailItem? ActiveVehicle { get; set; }
  public List<BookingHistoryItem> BookingHistory { get; set; } = [];
}

public class BookingRangeItem
{
  public DateTime StartTime { get; set; }
  public DateTime EndTime { get; set; }
}
