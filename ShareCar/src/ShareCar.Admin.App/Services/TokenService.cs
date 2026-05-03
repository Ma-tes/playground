using System.Net.Http.Json;
using ShareCar.Admin.App.Configuration;

namespace ShareCar.Admin.App.Services;

public sealed class TokenService
{
  private readonly HttpClient _httpClient;
  private readonly AdminApiClient _apiClient;
  private readonly AppSettings _settings;

  private sealed record LoginResponse(string Token);

  public bool IsAuthenticated { get; private set; }
  public string? ErrorMessage { get; private set; }

  public TokenService(IHttpClientFactory httpClientFactory, AdminApiClient apiClient, AppSettings settings)
  {
    _httpClient = httpClientFactory.CreateClient("auth");
    _apiClient = apiClient;
    _settings = settings;
  }

  public async Task<bool> AcquireTokenAsync()
  {
    ErrorMessage = null;
    try
    {
      var response = await _httpClient.PostAsJsonAsync(
        "api/auth/login",
        new { Username = _settings.AdminUsername, Password = _settings.AdminPassword });

      if (!response.IsSuccessStatusCode)
      {
        ErrorMessage = "Authentication failed. Check credentials in appsettings.json.";
        return false;
      }

      var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
      if (result?.Token is null)
      {
        ErrorMessage = "No token received from server.";
        return false;
      }

      _apiClient.SetBearerToken(result.Token);
      IsAuthenticated = true;
      return true;
    }
    catch (Exception ex)
    {
      ErrorMessage = $"Cannot connect to backend: {ex.Message}";
      return false;
    }
  }
}
