using ShareCar.Admin.App.ViewModels;

namespace ShareCar.Admin.App.Views;

public partial class BlockLogsPage : ContentPage
{
  public BlockLogsPage(BlockLogsViewModel viewModel)
  {
    InitializeComponent();
    BindingContext = viewModel;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    if (BindingContext is BlockLogsViewModel vm && !vm.IsBusy)
    {
      vm.LoadBlockedVehiclesCommand.Execute(null);
    }
  }
}
