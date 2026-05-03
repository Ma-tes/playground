using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShareCar.Admin.App.Models;
using ShareCar.Admin.App.Services;

namespace ShareCar.Admin.App.ViewModels;

public sealed partial class BlockLogsViewModel : BaseViewModel
{
  private readonly AdminApiClient _apiClient;

  [ObservableProperty]
  private ObservableCollection<BlockLogItem> _blockLogs = [];

  [ObservableProperty]
  private ObservableCollection<VehicleItem> _blockedVehicles = [];

  [ObservableProperty]
  private string _vehicleIdInput = string.Empty;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasVehicleIdError))]
  private string? _vehicleIdError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasBlockedVehicles))]
  private bool _blockedVehiclesLoaded;

  public bool HasVehicleIdError => VehicleIdError is not null;
  public bool HasBlockedVehicles => BlockedVehicles.Count > 0;

  public BlockLogsViewModel(AdminApiClient apiClient)
  {
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
  }

  [RelayCommand]
  private async Task LoadBlockedVehiclesAsync()
  {
    ClearMessages();
    IsBusy = true;
    try
    {
      var allVehicles = await _apiClient.GetAllVehiclesAsync();
      var blocked = allVehicles.Where(v => v.Status == "Blocked").ToList();
      BlockedVehicles = new ObservableCollection<VehicleItem>(blocked);
      BlockedVehiclesLoaded = true;
      OnPropertyChanged(nameof(HasBlockedVehicles));
    }
    finally
    {
      IsBusy = false;
    }
  }

  [RelayCommand]
  private async Task UnblockAsync(VehicleItem vehicle)
  {
    ClearMessages();
    IsBusy = true;
    try
    {
      var ok = await _apiClient.UnblockVehicleAsync(vehicle.Id);
      SuccessMessage = ok ? $"Vehicle #{vehicle.Id} unblocked." : null;
      ErrorMessage = ok ? null : _apiClient.LastError ?? "Unblock failed.";
      if (ok)
      {
        await LoadBlockedVehiclesAsync();
      }
    }
    finally
    {
      IsBusy = false;
    }
  }

  [RelayCommand]
  private async Task LoadAsync()
  {
    VehicleIdError = null;
    if (!int.TryParse(VehicleIdInput, out int vehicleId) || vehicleId <= 0)
    {
      VehicleIdError = "Enter a valid vehicle ID.";
      return;
    }

    ClearMessages();
    IsBusy = true;
    try
    {
      var items = await _apiClient.GetVehicleBlockHistoryAsync(vehicleId);
      if (items.Count == 0 && _apiClient.LastError is not null)
      {
        ErrorMessage = _apiClient.LastError;
        BlockLogs = [];
      }
      else
      {
        BlockLogs = new ObservableCollection<BlockLogItem>(items);
      }
    }
    finally
    {
      IsBusy = false;
    }
  }
}
