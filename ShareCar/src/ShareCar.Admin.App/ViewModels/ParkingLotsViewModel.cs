using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShareCar.Admin.App.Models;
using ShareCar.Admin.App.Services;

namespace ShareCar.Admin.App.ViewModels;

public sealed partial class ParkingLotsViewModel : BaseViewModel
{
  private readonly AdminApiClient _apiClient;
  private List<ParkingLotItem> _allLots = [];

  [ObservableProperty]
  private ObservableCollection<ParkingLotItem> _parkingLots = [];

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasSelection))]
  [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
  private ParkingLotItem? _selectedLot;

  [ObservableProperty]
  private string _editName = string.Empty;

  [ObservableProperty]
  private string _editLatitude = "0";

  [ObservableProperty]
  private string _editLongitude = "0";

  [ObservableProperty]
  private string _editCapacity = "0";

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasEditNameError))]
  private string? _editNameError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasEditLatitudeError))]
  private string? _editLatitudeError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasEditLongitudeError))]
  private string? _editLongitudeError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasEditCapacityError))]
  private string? _editCapacityError;

  [ObservableProperty]
  private bool _showCreatePanel;

  [ObservableProperty]
  private string _newName = string.Empty;

  [ObservableProperty]
  private string _newLatitude = "0";

  [ObservableProperty]
  private string _newLongitude = "0";

  [ObservableProperty]
  private string _newCapacity = "10";

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasNewNameError))]
  private string? _newNameError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasNewLatitudeError))]
  private string? _newLatitudeError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasNewLongitudeError))]
  private string? _newLongitudeError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasNewCapacityError))]
  private string? _newCapacityError;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SortIndicator))]
  private string _sortColumn = "Name";

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SortIndicator))]
  private bool _sortAscending = true;

  public bool HasSelection => SelectedLot is not null;
  public string SortIndicator => SortAscending ? "▲" : "▼";

  public bool HasEditNameError => EditNameError is not null;
  public bool HasEditLatitudeError => EditLatitudeError is not null;
  public bool HasEditLongitudeError => EditLongitudeError is not null;
  public bool HasEditCapacityError => EditCapacityError is not null;
  public bool HasNewNameError => NewNameError is not null;
  public bool HasNewLatitudeError => NewLatitudeError is not null;
  public bool HasNewLongitudeError => NewLongitudeError is not null;
  public bool HasNewCapacityError => NewCapacityError is not null;

  public ParkingLotsViewModel(AdminApiClient apiClient)
  {
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
  }

  partial void OnSelectedLotChanged(ParkingLotItem? value)
  {
    if (value is null)
    {
      return;
    }

    ShowCreatePanel = false;
    EditName = value.Name;
    EditLatitude = value.Latitude.ToString(CultureInfo.InvariantCulture);
    EditLongitude = value.Longitude.ToString(CultureInfo.InvariantCulture);
    EditCapacity = value.TotalCapacity.ToString(CultureInfo.InvariantCulture);
    ClearEditErrors();
  }

  [RelayCommand]
  private async Task LoadAsync()
  {
    ClearMessages();
    IsBusy = true;
    try
    {
      _allLots = await _apiClient.GetParkingLotsAsync();
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
    SelectedLot = null;
    ShowCreatePanel = false;
  }

  [RelayCommand]
  private void ToggleCreatePanel()
  {
    ShowCreatePanel = !ShowCreatePanel;
    if (ShowCreatePanel)
    {
      SelectedLot = null;
    }
  }

  [RelayCommand(CanExecute = nameof(HasSelection))]
  private async Task SaveAsync()
  {
    if (SelectedLot is null || !ValidateEditFields())
    {
      return;
    }

    ClearMessages();
    IsBusy = true;
    try
    {
      double lat = double.Parse(EditLatitude, CultureInfo.InvariantCulture);
      double lon = double.Parse(EditLongitude, CultureInfo.InvariantCulture);
      int cap = int.Parse(EditCapacity, CultureInfo.InvariantCulture);
      var ok = await _apiClient.UpdateParkingLotAsync(SelectedLot.Id, EditName.Trim(), lat, lon, cap);
      SuccessMessage = ok ? "Parking lot updated." : null;
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
      double lat = double.Parse(NewLatitude, CultureInfo.InvariantCulture);
      double lon = double.Parse(NewLongitude, CultureInfo.InvariantCulture);
      int cap = int.Parse(NewCapacity, CultureInfo.InvariantCulture);
      var ok = await _apiClient.CreateParkingLotAsync(NewName.Trim(), lat, lon, cap);
      SuccessMessage = ok ? "Parking lot created." : null;
      ErrorMessage = ok ? null : _apiClient.LastError ?? "Create failed.";
      if (ok)
      {
        ShowCreatePanel = false;
        NewName = string.Empty;
        NewLatitude = "0";
        NewLongitude = "0";
        NewCapacity = "10";
        await LoadAsync();
      }
    }
    finally
    {
      IsBusy = false;
    }
  }

  [RelayCommand]
  private async Task DeleteAsync(ParkingLotItem lot)
  {
    ClearMessages();
    IsBusy = true;
    try
    {
      var ok = await _apiClient.DeleteParkingLotAsync(lot.Id);
      SuccessMessage = ok ? "Parking lot deleted." : null;
      ErrorMessage = ok ? null : _apiClient.LastError ?? "Delete failed.";
      if (ok)
      {
        if (SelectedLot?.Id == lot.Id)
        {
          SelectedLot = null;
        }

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
    Func<ParkingLotItem, object> key = SortColumn switch
    {
      "Capacity" => l => (object)l.TotalCapacity,
      "Available" => l => (object)l.AvailableVehicles,
      _ => l => l.Name,
    };

    ParkingLots = new ObservableCollection<ParkingLotItem>(
      SortAscending ? _allLots.OrderBy(key) : _allLots.OrderByDescending(key));
  }

  private bool ValidateEditFields()
  {
    EditNameError = EditName.Trim().Length >= 2 ? null : "Name must be at least 2 characters.";
    EditLatitudeError = double.TryParse(EditLatitude, CultureInfo.InvariantCulture, out _) ? null : "Invalid latitude.";
    EditLongitudeError = double.TryParse(EditLongitude, CultureInfo.InvariantCulture, out _) ? null : "Invalid longitude.";
    EditCapacityError = int.TryParse(EditCapacity, out int c) && c > 0 ? null : "Capacity must be a positive integer.";
    return EditNameError is null && EditLatitudeError is null && EditLongitudeError is null && EditCapacityError is null;
  }

  private bool ValidateNewFields()
  {
    NewNameError = NewName.Trim().Length >= 2 ? null : "Name must be at least 2 characters.";
    NewLatitudeError = double.TryParse(NewLatitude, CultureInfo.InvariantCulture, out _) ? null : "Invalid latitude.";
    NewLongitudeError = double.TryParse(NewLongitude, CultureInfo.InvariantCulture, out _) ? null : "Invalid longitude.";
    NewCapacityError = int.TryParse(NewCapacity, out int c) && c > 0 ? null : "Capacity must be a positive integer.";
    return NewNameError is null && NewLatitudeError is null && NewLongitudeError is null && NewCapacityError is null;
  }

  private void ClearEditErrors()
  {
    EditNameError = null;
    EditLatitudeError = null;
    EditLongitudeError = null;
    EditCapacityError = null;
  }
}
