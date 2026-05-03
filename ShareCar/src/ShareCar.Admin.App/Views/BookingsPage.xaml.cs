using ShareCar.Admin.App.ViewModels;

namespace ShareCar.Admin.App.Views;

public partial class BookingsPage : ContentPage
{
  public BookingsPage(BookingsViewModel viewModel)
  {
    InitializeComponent();
    BindingContext = viewModel;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    if (BindingContext is BookingsViewModel vm && !vm.IsBusy)
    {
      vm.LoadCommand.Execute(null);
    }
  }
}
