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
        },
        new Tab
        {
          Title = "VEHICLES",
          Items =
          {
            new ShellContent
            {
              ContentTemplate = new DataTemplate(() => services.GetRequiredService<VehiclesPage>())
            }
          }
        },
        new Tab
        {
          Title = "LOTS",
          Items =
          {
            new ShellContent
            {
              ContentTemplate = new DataTemplate(() => services.GetRequiredService<ParkingLotsPage>())
            }
          }
        },
        new Tab
        {
          Title = "BOOKINGS",
          Items =
          {
            new ShellContent
            {
              ContentTemplate = new DataTemplate(() => services.GetRequiredService<BookingsPage>())
            }
          }
        },
        new Tab
        {
          Title = "HISTORY",
          Items =
          {
            new ShellContent
            {
              ContentTemplate = new DataTemplate(() => services.GetRequiredService<StatusHistoryPage>())
            }
          }
        },
        new Tab
        {
          Title = "BLOCKS",
          Items =
          {
            new ShellContent
            {
              ContentTemplate = new DataTemplate(() => services.GetRequiredService<BlockLogsPage>())
            }
          }
        },
        new Tab
        {
          Title = "STATS",
          Items =
          {
            new ShellContent
            {
              ContentTemplate = new DataTemplate(() => services.GetRequiredService<StatisticsPage>())
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
    {
      await DisplayAlert("Authentication Failed", _tokenService.ErrorMessage, "OK");
    }
  }
}
