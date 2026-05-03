using ShareCar.Admin.App.ViewModels;

namespace ShareCar.Admin.App.Views;

public partial class StatusHistoryPage : ContentPage
{
  public StatusHistoryPage(StatusHistoryViewModel viewModel)
  {
    InitializeComponent();
    BindingContext = viewModel;
  }
}
