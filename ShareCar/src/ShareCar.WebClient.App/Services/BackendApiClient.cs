using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ShareCar.WebClient.App.Models;

namespace ShareCar.WebClient.App.Services;

public class BackendApiClient
{
  private readonly HttpClient _client;
  private readonly IHttpContextAccessor _httpContextAccessor;

  private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

  public BackendApiClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
  {
    _client = client ?? throw new ArgumentNullException(nameof(client));
    _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    AttachJwtToken();
  }

  public async Task<List<ParkingLotItem>> GetParkingLotsAsync()
  {
    var response = await _client.GetAsync("api/parkinglots");
    if (!response.IsSuccessStatusCode)
    {
      return [];
    }

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<List<ParkingLotItem>>(json, JsonOptions) ?? [];
  }

  public async Task<List<VehicleItem>> GetVehiclesByParkingLotAsync(int parkingLotId)
  {
    var response = await _client.GetAsync($"api/vehicles/by-parking-lot/{parkingLotId}");
    if (!response.IsSuccessStatusCode)
    {
      return [];
    }

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<List<VehicleItem>>(json, JsonOptions) ?? [];
  }

  public async Task<ActiveBookingItem?> GetActiveBookingAsync()
  {
    var response = await _client.GetAsync("api/bookings/active");
    if (!response.IsSuccessStatusCode)
    {
      return null;
    }

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<ActiveBookingItem>(json, JsonOptions);
  }

  public async Task<RentResultItem?> RentVehicleAsync(int vehicleId)
  {
    var payload = JsonSerializer.Serialize(new { VehicleId = vehicleId });
    var content = new StringContent(payload, Encoding.UTF8, "application/json");

    var response = await _client.PostAsync("api/bookings/rent", content);
    if (!response.IsSuccessStatusCode)
    {
      LastErrorMessage = await ExtractErrorMessageAsync(response);

      return null;
    }

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<RentResultItem>(json, JsonOptions);
  }

  public async Task<ReturnResultItem?> ReturnVehicleAsync(int bookingId, int parkingLotId, int endOdometer)
  {
    var payload = JsonSerializer.Serialize(new { ParkingLotId = parkingLotId, EndOdometer = endOdometer });
    var content = new StringContent(payload, Encoding.UTF8, "application/json");

    var response = await _client.PostAsync($"api/bookings/{bookingId}/return", content);
    if (!response.IsSuccessStatusCode)
    {
      LastErrorMessage = await ExtractErrorMessageAsync(response);

      return null;
    }

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<ReturnResultItem>(json, JsonOptions);
  }

  public async Task<List<BookingHistoryItem>> GetMyBookingsAsync()
  {
    var response = await _client.GetAsync("api/bookings/my");
    if (!response.IsSuccessStatusCode)
    {
      return [];
    }

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<List<BookingHistoryItem>>(json, JsonOptions) ?? [];
  }

  public async Task<VehicleDetailItem?> GetVehicleByIdAsync(int vehicleId)
  {
    var response = await _client.GetAsync($"api/vehicles/{vehicleId}");
    if (!response.IsSuccessStatusCode)
    {
      return null;
    }

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<VehicleDetailItem>(json, JsonOptions);
  }

  public async Task<VehicleStatisticsItem?> GetVehicleStatisticsAsync(int vehicleId)
  {
    var response = await _client.GetAsync($"api/vehicles/{vehicleId}/statistics");
    if (!response.IsSuccessStatusCode)
    {
      return null;
    }

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<VehicleStatisticsItem>(json, JsonOptions);
  }

  public async Task<string?> LoginAsync(string username, string password)
  {
    var payload = JsonSerializer.Serialize(new { Username = username, Password = password });
    var content = new StringContent(payload, Encoding.UTF8, "application/json");

    var response = await _client.PostAsync("api/auth/login", content);
    if (!response.IsSuccessStatusCode)
    {
      return null;
    }

    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<JsonElement>(json);

    return result.GetProperty("token").GetString();
  }

  public async Task<bool> RegisterAsync(string username, string password, string email)
  {
    var payload = JsonSerializer.Serialize(new { Username = username, Password = password, Email = email });
    var content = new StringContent(payload, Encoding.UTF8, "application/json");

    var response = await _client.PostAsync("api/auth/register", content);

    return response.IsSuccessStatusCode;
  }

  public string? LastErrorMessage { get; private set; }

  private void AttachJwtToken()
  {
    var jwt = _httpContextAccessor.HttpContext?.User.FindFirstValue("jwt");
    if (!string.IsNullOrEmpty(jwt))
    {
      _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    }
  }

  private static async Task<string?> ExtractErrorMessageAsync(HttpResponseMessage response)
  {
    try
    {
      var json = await response.Content.ReadAsStringAsync();
      var doc = JsonSerializer.Deserialize<JsonElement>(json);
      if (doc.TryGetProperty("message", out var msg))
      {
        return msg.GetString();
      }
    }
    catch
    {
      // Ignore deserialization errors
    }

    return null;
  }
}
