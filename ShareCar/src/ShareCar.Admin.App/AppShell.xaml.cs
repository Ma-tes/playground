using ShareCar.Admin.App.Services;
using ShareCar.Admin.App.Views;

namespace ShareCar.Admin.App;

public partial class AppShell : Shell
{
    private readonly TokenService _tokenService;

    public AppShell(IServiceProvider services, TokenService tokenService)
    {
        _tokenService = tokenService;
        InitializeComponent();

        Items.Add(new TabBar
        {
            Items =
            {
                new Tab
                {
                    Title = "USERS",
                    Items =
                    {
                        new ShellContent
                        {
                            ContentTemplate = new DataTemplate(() => services.GetRequiredService<UsersPage>())
                        }
                    }
                }
            }
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var ok = await _tokenService.AcquireTokenAsync();
        if (!ok)
            await DisplayAlert("Authentication Failed", _tokenService.ErrorMessage, "OK");
    }
}
