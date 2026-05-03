using ShareCar.Admin.App.ViewModels;

namespace ShareCar.Admin.App.Views;

public partial class ParkingLotsPage : ContentPage
{
  public ParkingLotsPage(ParkingLotsViewModel viewModel)
  {
    InitializeComponent();
    BindingContext = viewModel;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    if (BindingContext is ParkingLotsViewModel vm && !vm.IsBusy)
    {
      vm.LoadCommand.Execute(null);
    }
  }
}
