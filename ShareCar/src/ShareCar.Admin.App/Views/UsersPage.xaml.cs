using ShareCar.Admin.App.ViewModels;

namespace ShareCar.Admin.App.Views;

public partial class UsersPage : ContentPage
{
    public UsersPage(UsersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UsersViewModel vm && !vm.IsBusy)
            vm.LoadCommand.Execute(null);
    }
}
