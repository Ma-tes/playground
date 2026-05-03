using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShareCar.Admin.App.Configuration;
using ShareCar.Admin.App.Services;
using ShareCar.Admin.App.ViewModels;
using ShareCar.Admin.App.Views;

namespace ShareCar.Admin.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();

        LoadConfiguration(builder);

        var settings = builder.Configuration.GetSection("Backend").Get<AppSettings>()
            ?? throw new InvalidOperationException("Backend configuration section is missing.");

        builder.Services.AddSingleton(settings);

        RegisterHttpClients(builder, settings);

        builder.Services
            .AddSingleton<AdminApiClient>()
            .AddSingleton<TokenService>()
            .AddTransient<UsersViewModel>()
            .AddTransient<UsersPage>()
            .AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void LoadConfiguration(MauiAppBuilder builder)
    {
        using var stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("ShareCar.Admin.App.appsettings.json");

        if (stream is not null)
            builder.Configuration.AddJsonStream(stream);
    }

    private static void RegisterHttpClients(MauiAppBuilder builder, AppSettings settings)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        builder.Services.AddHttpClient("auth", client =>
            client.BaseAddress = new Uri(settings.BaseUrl))
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        builder.Services.AddHttpClient<AdminApiClient>(client =>
            client.BaseAddress = new Uri(settings.BaseUrl))
            .ConfigurePrimaryHttpMessageHandler(() => handler);
    }
}
