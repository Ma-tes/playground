using CommunityToolkit.Mvvm.ComponentModel;

namespace ShareCar.Admin.App.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(IsNotBusy))]
  private bool _isBusy;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasError))]
  private string? _errorMessage;

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(HasSuccess))]
  private string? _successMessage;

  public bool IsNotBusy => !IsBusy;
  public bool HasError => ErrorMessage is not null;
  public bool HasSuccess => SuccessMessage is not null;

  protected void ClearMessages()
  {
    ErrorMessage = null;
    SuccessMessage = null;
  }
}
