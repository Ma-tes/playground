using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShareCar.Admin.App.Models;
using ShareCar.Admin.App.Services;

namespace ShareCar.Admin.App.ViewModels;

public sealed partial class StatusHistoryViewModel : BaseViewModel
{
  private readonly AdminApiClient _apiClient;

  [ObservableProperty]
  private ObservableCollection<StatusHistoryItem> _history = [];

  [ObservableProperty]
  private string _vehicleIdInput = string.Empty;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasVehicleIdError))]
  private string? _vehicleIdError;

  public bool HasVehicleIdError => VehicleIdError is not null;

  public StatusHistoryViewModel(AdminApiClient apiClient)
  {
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
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
      var items = await _apiClient.GetVehicleStatusHistoryAsync(vehicleId);
      if (items.Count == 0 && _apiClient.LastError is not null)
      {
        ErrorMessage = _apiClient.LastError;
        History = [];
      }
      else
      {
        History = new ObservableCollection<StatusHistoryItem>(items);
      }
    }
    finally
    {
      IsBusy = false;
    }
  }
}
