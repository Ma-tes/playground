using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShareCar.Admin.App.Models;
using ShareCar.Admin.App.Services;

namespace ShareCar.Admin.App.ViewModels;

public sealed partial class VehiclesViewModel : BaseViewModel
{
  private readonly AdminApiClient _apiClient;
  private List<VehicleItem> _allVehicles = [];

  public static readonly ParkingLotItem NoLot = new(-1, "(None)", 0, 0, 0, 0);
  public ObservableCollection<ParkingLotItem> ParkingLots { get; } = [NoLot];

  [ObservableProperty]
  private ObservableCollection<VehicleItem> _vehicles = [];

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasSelection))]
  [NotifyPropertyChangedFor(nameof(CanBlock))]
  [NotifyPropertyChangedFor(nameof(CanUnblock))]
  [NotifyPropertyChangedFor(nameof(CurrentStatusLabel))]
  [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
  [NotifyCanExecuteChangedFor(nameof(BlockCommand))]
  [NotifyCanExecuteChangedFor(nameof(UnblockCommand))]
  private VehicleItem? _selectedVehicle;

  [ObservableProperty]
  private string _editModel = string.Empty;

  [ObservableProperty]
  private string _editPlate = string.Empty;

  [ObservableProperty]
  private string _editOdometer = "0";

  [ObservableProperty]
  private ParkingLotItem? _selectedEditLot;

  [ObservableProperty]
  private string _blockReason = string.Empty;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasEditModelError))]
  private string? _editModelError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasEditPlateError))]
  private string? _editPlateError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasEditOdometerError))]
  private string? _editOdometerError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasBlockReasonError))]
  private string? _blockReasonError;

  [ObservableProperty]
  private bool _showCreatePanel;

  [ObservableProperty]
  private string _newModel = string.Empty;

  [ObservableProperty]
  private string _newPlate = string.Empty;

  [ObservableProperty]
  private string _newOdometer = "0";

  [ObservableProperty]
  private ParkingLotItem? _selectedNewLot;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasNewModelError))]
  private string? _newModelError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasNewPlateError))]
  private string? _newPlateError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasNewOdometerError))]
  private string? _newOdometerError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SortIndicator))]
  private string _sortColumn = "Model";

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SortIndicator))]
  private bool _sortAscending = true;

  public bool HasSelection => SelectedVehicle is not null;
  public bool CanBlock => SelectedVehicle is not null && SelectedVehicle.Status != "Blocked";
  public bool CanUnblock => SelectedVehicle?.Status == "Blocked";
  public string CurrentStatusLabel => SelectedVehicle?.Status ?? string.Empty;
  public string SortIndicator => SortAscending ? "▲" : "▼";

  public bool HasEditModelError => EditModelError is not null;
  public bool HasEditPlateError => EditPlateError is not null;
  public bool HasEditOdometerError => EditOdometerError is not null;
  public bool HasBlockReasonError => BlockReasonError is not null;
  public bool HasNewModelError => NewModelError is not null;
  public bool HasNewPlateError => NewPlateError is not null;
  public bool HasNewOdometerError => NewOdometerError is not null;

  public VehiclesViewModel(AdminApiClient apiClient)
  {
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
  }

  partial void OnSelectedVehicleChanged(VehicleItem? value)
  {
    if (value is null)
    {
      return;
    }

    ShowCreatePanel = false;
    EditModel = value.Model;
    EditPlate = value.PlateNumber;
    EditOdometer = value.Odometer.ToString(CultureInfo.InvariantCulture);
    SelectedEditLot = ParkingLots.FirstOrDefault(p => p.Id == value.CurrentParkingLotId) ?? NoLot;
    BlockReason = string.Empty;
    BlockReasonError = null;
    EditModelError = null;
    EditPlateError = null;
    EditOdometerError = null;
  }

  [RelayCommand]
  private async Task LoadAsync()
  {
    ClearMessages();
    IsBusy = true;
    try
    {
      var lots = await _apiClient.GetParkingLotsAsync();
      ParkingLots.Clear();
      ParkingLots.Add(NoLot);
      foreach (var lot in lots)
      {
        ParkingLots.Add(lot);
      }

      if (SelectedVehicle is not null)
      {
        SelectedEditLot = ParkingLots.FirstOrDefault(p => p.Id == SelectedVehicle.CurrentParkingLotId) ?? NoLot;
      }

      _allVehicles = await _apiClient.GetAllVehiclesAsync();
      ApplySort();
    }
    finally
    {
      IsBusy = false;
    }
  }

  [RelayCommand]
  private void Sort(string column)
  {
    if (SortColumn == column)
    {
      SortAscending = !SortAscending;
    }
    else
    {
      SortColumn = column;
      SortAscending = true;
    }

    ApplySort();
  }

  [RelayCommand]
  private void ClearSelection()
  {
    SelectedVehicle = null;
    ShowCreatePanel = false;
  }

  [RelayCommand]
  private void ToggleCreatePanel()
  {
    ShowCreatePanel = !ShowCreatePanel;
    if (ShowCreatePanel)
    {
      SelectedVehicle = null;
    }
  }

  [RelayCommand(CanExecute = nameof(HasSelection))]
  private async Task SaveAsync()
  {
    if (SelectedVehicle is null || !ValidateEditFields())
    {
      return;
    }

    ClearMessages();
    IsBusy = true;
    try
    {
      int? lotId = SelectedEditLot?.Id == -1 ? null : SelectedEditLot?.Id;
      int odometer = int.Parse(EditOdometer, CultureInfo.InvariantCulture);
      var ok = await _apiClient.UpdateVehicleAsync(SelectedVehicle.Id, EditModel.Trim(), EditPlate.Trim(), lotId, odometer);
      SuccessMessage = ok ? "Vehicle updated." : null;
      ErrorMessage = ok ? null : _apiClient.LastError ?? "Update failed.";
      if (ok)
      {
        await LoadAsync();
      }
    }
    finally
    {
      IsBusy = false;
    }
  }

  [RelayCommand]
  private async Task DeleteAsync(VehicleItem vehicle)
  {
    ClearMessages();
    IsBusy = true;
    try
    {
      var ok = await _apiClient.DeleteVehicleAsync(vehicle.Id);
      SuccessMessage = ok ? "Vehicle deleted." : null;
      ErrorMessage = ok ? null : _apiClient.LastError ?? "Delete failed.";
      if (ok)
      {
        if (SelectedVehicle?.Id == vehicle.Id)
        {
          SelectedVehicle = null;
        }

        await LoadAsync();
      }
    }
    finally
    {
      IsBusy = false;
    }
  }

  [RelayCommand]
  private async Task CreateAsync()
  {
    if (!ValidateNewFields())
    {
      return;
    }

    ClearMessages();
    IsBusy = true;
    try
    {
      int? lotId = SelectedNewLot?.Id == -1 ? null : SelectedNewLot?.Id;
      int odometer = int.Parse(NewOdometer, CultureInfo.InvariantCulture);
      var ok = await _apiClient.CreateVehicleAsync(NewModel.Trim(), NewPlate.Trim(), lotId, odometer);
      SuccessMessage = ok ? "Vehicle created." : null;
      ErrorMessage = ok ? null : _apiClient.LastError ?? "Create failed.";
      if (ok)
      {
        ShowCreatePanel = false;
        NewModel = string.Empty;
        NewPlate = string.Empty;
        NewOdometer = "0";
        SelectedNewLot = null;
        await LoadAsync();
      }
    }
    finally
    {
      IsBusy = false;
    }
  }

  [RelayCommand(CanExecute = nameof(CanBlock))]
  private async Task BlockAsync()
  {
    if (SelectedVehicle is null)
    {
      return;
    }

    BlockReasonError = string.IsNullOrWhiteSpace(BlockReason) ? "Block reason is required." : null;
    if (BlockReasonError is not null)
    {
      return;
    }

    ClearMessages();
    IsBusy = true;
    try
    {
      var ok = await _apiClient.BlockVehicleAsync(SelectedVehicle.Id, BlockReason.Trim());
      SuccessMessage = ok ? "Vehicle blocked." : null;
      ErrorMessage = ok ? null : _apiClient.LastError ?? "Block failed.";
      if (ok)
      {
        BlockReason = string.Empty;
        await LoadAsync();
      }
    }
    finally
    {
      IsBusy = false;
    }
  }

  [RelayCommand(CanExecute = nameof(CanUnblock))]
  private async Task UnblockAsync()
  {
    if (SelectedVehicle is null)
    {
      return;
    }

    ClearMessages();
    IsBusy = true;
    try
    {
      var ok = await _apiClient.UnblockVehicleAsync(SelectedVehicle.Id);
      SuccessMessage = ok ? "Vehicle unblocked." : null;
      ErrorMessage = ok ? null : _apiClient.LastError ?? "Unblock failed.";
      if (ok)
      {
        await LoadAsync();
      }
    }
    finally
    {
      IsBusy = false;
    }
  }

  private void ApplySort()
  {
    Func<VehicleItem, object> key = SortColumn switch
    {
      "Plate" => v => v.PlateNumber,
      "Status" => v => v.Status,
      "Odometer" => v => (object)v.Odometer,
      _ => v => v.Model,
    };

    Vehicles = new ObservableCollection<VehicleItem>(
      SortAscending ? _allVehicles.OrderBy(key) : _allVehicles.OrderByDescending(key));
  }

  private bool ValidateEditFields()
  {
    EditModelError = EditModel.Trim().Length >= 2 ? null : "Model must be at least 2 characters.";
    EditPlateError = EditPlate.Trim().Length >= 3 ? null : "Plate must be at least 3 characters.";
    EditOdometerError = int.TryParse(EditOdometer, out int v) && v >= 0 ? null : "Odometer must be a non-negative integer.";
    return EditModelError is null && EditPlateError is null && EditOdometerError is null;
  }

  private bool ValidateNewFields()
  {
    NewModelError = NewModel.Trim().Length >= 2 ? null : "Model must be at least 2 characters.";
    NewPlateError = NewPlate.Trim().Length >= 3 ? null : "Plate must be at least 3 characters.";
    NewOdometerError = int.TryParse(NewOdometer, out int v) && v >= 0 ? null : "Odometer must be a non-negative integer.";
    return NewModelError is null && NewPlateError is null && NewOdometerError is null;
  }
}
