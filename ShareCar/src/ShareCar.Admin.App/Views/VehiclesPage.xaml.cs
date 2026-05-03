using ShareCar.Admin.App.ViewModels;

namespace ShareCar.Admin.App.Views;

public partial class VehiclesPage : ContentPage
{
  public VehiclesPage(VehiclesViewModel viewModel)
  {
    InitializeComponent();
    BindingContext = viewModel;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    if (BindingContext is VehiclesViewModel vm && !vm.IsBusy)
    {
      vm.LoadCommand.Execute(null);
    }
  }
}
