namespace ShareCar.Admin.App.Models;

public sealed record UserItem(int Id, string Username, string Email, string Role);

public sealed record VehicleItem(
  int Id,
  string Model,
  string PlateNumber,
  int? CurrentParkingLotId,
  string Status,
  int Odometer);

public sealed record ParkingLotItem(
  int Id,
  string Name,
  double Latitude,
  double Longitude,
  int TotalCapacity,
  int AvailableVehicles);

public sealed record BookingItem(
  int Id,
  int UserId,
  int VehicleId,
  int StartParkingLotId,
  DateTime StartTime,
  DateTime? EndTime,
  int StartOdometer,
  int? EndOdometer,
  decimal? TotalPrice,
  bool IsActive);

public sealed record BlockLogItem(
  int Id,
  int VehicleId,
  int AdminId,
  string Reason,
  DateTime StartTime,
  DateTime? EndTime,
  bool IsActive);

public sealed record StatusHistoryItem(
  int Id,
  int VehicleId,
  string OldStatus,
  string NewStatus,
  DateTime ChangedAt,
  string TriggeredBy);

public sealed record StatisticsOverview(
  int TotalUsers,
  int TotalVehicles,
  int TotalParkingLots,
  int ActiveBookings,
  int TotalBookings);

public sealed record VehicleStatistics(int TotalTrips, int TotalDistanceKm);

public sealed record PagedResult<T>(int Total, int Page, int PageSize, IReadOnlyList<T> Items);
