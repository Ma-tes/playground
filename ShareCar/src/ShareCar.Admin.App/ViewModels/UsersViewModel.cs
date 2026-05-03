using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShareCar.Admin.App.Models;
using ShareCar.Admin.App.Services;

namespace ShareCar.Admin.App.ViewModels;

public sealed partial class UsersViewModel : BaseViewModel
{
  private readonly AdminApiClient _apiClient;
  private List<UserItem> _rawUsers = [];

  [ObservableProperty]
  private ObservableCollection<UserItem> _users = [];

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasSelection))]
  [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
  private UserItem? _selectedUser;

  [ObservableProperty]
  private string _editEmail = string.Empty;

  [ObservableProperty]
  private string _editRole = "User";

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SortAscendingIndicator))]
  private string _sortColumn = "Username";

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SortAscendingIndicator))]
  private bool _sortAscending = true;

  public bool HasSelection => SelectedUser is not null;
  public string SortAscendingIndicator => SortAscending ? "▲" : "▼";

  public IReadOnlyList<string> AvailableRoles { get; } = ["User", "Admin"];

  public UsersViewModel(AdminApiClient apiClient)
  {
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
  }

  partial void OnSelectedUserChanged(UserItem? value)
  {
    if (value is null)
    {
      return;
    }

    EditEmail = value.Email;
    EditRole = value.Role;
  }

  [RelayCommand]
  private async Task LoadAsync()
  {
    ClearMessages();
    IsBusy = true;
    try
    {
      _rawUsers = await _apiClient.GetUsersAsync();
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
    SelectedUser = null;
  }

  [RelayCommand(CanExecute = nameof(HasSelection))]
  private async Task SaveAsync()
  {
    if (SelectedUser is null)
    {
      return;
    }

    ClearMessages();
    IsBusy = true;
    try
    {
      var ok = await _apiClient.UpdateUserAsync(SelectedUser.Id, EditEmail, EditRole);
      SuccessMessage = ok ? "User updated." : null;
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
  private async Task DeleteAsync(UserItem user)
  {
    ClearMessages();
    IsBusy = true;
    try
    {
      var ok = await _apiClient.DeleteUserAsync(user.Id);
      SuccessMessage = ok ? "User deleted." : null;
      ErrorMessage = ok ? null : _apiClient.LastError ?? "Delete failed.";
      if (ok)
      {
        if (SelectedUser?.Id == user.Id)
        {
          SelectedUser = null;
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
    Func<UserItem, string> key = SortColumn switch
    {
      "Email" => u => u.Email,
      "Role" => u => u.Role,
      _ => u => u.Username
    };

    var sorted = SortAscending
      ? _rawUsers.OrderBy(key)
      : _rawUsers.OrderByDescending(key);

    Users = new ObservableCollection<UserItem>(sorted);
  }
}
