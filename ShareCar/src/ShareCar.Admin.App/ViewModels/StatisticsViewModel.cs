using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShareCar.Admin.App.Models;
using ShareCar.Admin.App.Services;

namespace ShareCar.Admin.App.ViewModels;

public sealed partial class StatisticsViewModel : BaseViewModel
{
  private readonly AdminApiClient _apiClient;

  [ObservableProperty]
  private int _totalUsers;

  [ObservableProperty]
  private int _totalVehicles;

  [ObservableProperty]
  private int _totalParkingLots;

  [ObservableProperty]
  private int _activeBookings;

  [ObservableProperty]
  private int _totalBookings;

  [ObservableProperty]
  private string _vehicleIdInput = string.Empty;

  [ObservableProperty]
  private int _vehicleTrips;

  [ObservableProperty]
  private int _vehicleDistanceKm;

  [ObservableProperty]
  private bool _hasVehicleStats;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasVehicleIdError))]
  private string? _vehicleIdError;

  public bool HasVehicleIdError => VehicleIdError is not null;

  public StatisticsViewModel(AdminApiClient apiClient)
  {
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
  }

  [RelayCommand]
  private async Task LoadAsync()
  {
    ClearMessages();
    IsBusy = true;
    try
    {
      var overview = await _apiClient.GetStatisticsOverviewAsync();
      if (overview is null)
      {
        ErrorMessage = _apiClient.LastError ?? "Failed to load statistics.";
        return;
      }

      TotalUsers = overview.TotalUsers;
      TotalVehicles = overview.TotalVehicles;
      TotalParkingLots = overview.TotalParkingLots;
      ActiveBookings = overview.ActiveBookings;
      TotalBookings = overview.TotalBookings;
    }
    finally
    {
      IsBusy = false;
    }
  }

  [RelayCommand]
  private async Task LoadVehicleStatsAsync()
  {
    VehicleIdError = null;
    HasVehicleStats = false;

    if (!int.TryParse(VehicleIdInput, out int vehicleId) || vehicleId <= 0)
    {
      VehicleIdError = "Enter a valid vehicle ID.";
      return;
    }

    IsBusy = true;
    try
    {
      var stats = await _apiClient.GetVehicleStatisticsAsync(vehicleId);
      if (stats is null)
      {
        ErrorMessage = _apiClient.LastError ?? "Failed to load vehicle statistics.";
        return;
      }

      VehicleTrips = stats.TotalTrips;
      VehicleDistanceKm = stats.TotalDistanceKm;
      HasVehicleStats = true;
    }
    finally
    {
      IsBusy = false;
    }
  }
}
