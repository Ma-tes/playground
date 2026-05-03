using ShareCar.Admin.App.ViewModels;

namespace ShareCar.Admin.App.Views;

public partial class StatisticsPage : ContentPage
{
  public StatisticsPage(StatisticsViewModel viewModel)
  {
    InitializeComponent();
    BindingContext = viewModel;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    if (BindingContext is StatisticsViewModel vm && !vm.IsBusy)
    {
      vm.LoadCommand.Execute(null);
    }
  }
}
