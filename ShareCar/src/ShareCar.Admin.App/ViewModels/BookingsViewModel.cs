using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShareCar.Admin.App.Models;
using ShareCar.Admin.App.Services;

namespace ShareCar.Admin.App.ViewModels;

public sealed partial class BookingsViewModel : BaseViewModel
{
  private readonly AdminApiClient _apiClient;

  [ObservableProperty]
  private ObservableCollection<BookingItem> _bookings = [];

  [ObservableProperty]
  private int _currentPage = 1;

  [ObservableProperty]
  private int _totalPages = 1;

  [ObservableProperty]
  private string _sortColumn = "StartTime";

  [ObservableProperty]
  private bool _sortAscending;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasSelection))]
  private BookingItem? _selectedBooking;

  public bool HasSelection => SelectedBooking is not null;
  public bool CanGoBack => CurrentPage > 1;
  public bool CanGoForward => CurrentPage < TotalPages;

  public BookingsViewModel(AdminApiClient apiClient)
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
      string sortDir = SortAscending ? "asc" : "desc";
      var result = await _apiClient.GetBookingsAsync(CurrentPage, 50, SortColumn.ToLowerInvariant(), sortDir);
      if (result is null)
      {
        ErrorMessage = _apiClient.LastError ?? "Failed to load bookings.";
        return;
      }

      Bookings = new ObservableCollection<BookingItem>(result.Items);
      TotalPages = (int)Math.Ceiling((double)result.Total / result.PageSize);
      if (TotalPages < 1)
      {
        TotalPages = 1;
      }

      OnPropertyChanged(nameof(CanGoBack));
      OnPropertyChanged(nameof(CanGoForward));
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
      SortAscending = false;
    }

    CurrentPage = 1;
    LoadCommand.Execute(null);
  }

  [RelayCommand]
  private void NextPage()
  {
    if (CurrentPage < TotalPages)
    {
      CurrentPage++;
      LoadCommand.Execute(null);
    }
  }

  [RelayCommand]
  private void PrevPage()
  {
    if (CurrentPage > 1)
    {
      CurrentPage--;
      LoadCommand.Execute(null);
    }
  }

  [RelayCommand]
  private void ClearSelection()
  {
    SelectedBooking = null;
  }
}
