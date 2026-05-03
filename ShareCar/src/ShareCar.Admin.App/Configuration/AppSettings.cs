namespace ShareCar.Admin.App.Configuration;

public sealed class AppSettings
{
    public string BaseUrl { get; init; } = "https://localhost:7153";
    public string AdminUsername { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
}
