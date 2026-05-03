using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ShareCar.Admin.App.Models;

namespace ShareCar.Admin.App.Services;

public sealed class AdminApiClient
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AdminApiClient(HttpClient client) =>
        _client = client ?? throw new ArgumentNullException(nameof(client));

    public void SetBearerToken(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public string? LastError { get; private set; }

    public async Task<List<UserItem>> GetUsersAsync()
        => await GetListAsync<UserItem>("api/users");

    public async Task<bool> UpdateUserAsync(int id, string email, string role)
        => await PutAsync($"api/users/{id}", new { Email = email, Role = role });

    public async Task<bool> DeleteUserAsync(int id)
        => await DeleteAsync($"api/users/{id}");

    public async Task<List<VehicleItem>> GetVehiclesAsync()
        => await GetListAsync<VehicleItem>("api/vehicles/search?parkingLotIds=");

    public async Task<List<VehicleItem>> GetAllVehiclesAsync()
    {
        var lots = await GetListAsync<ParkingLotItem>("api/parkinglots");
        var lotIds = string.Join("&parkingLotIds=", lots.Select(l => l.Id));
        if (string.IsNullOrEmpty(lotIds))
            return [];
        return await GetListAsync<VehicleItem>($"api/vehicles/search?parkingLotIds={lotIds}");
    }

    public async Task<bool> CreateVehicleAsync(string model, string plateNumber, int? parkingLotId, int odometer)
        => await PostBoolAsync("api/vehicles", new { Model = model, PlateNumber = plateNumber, CurrentParkingLotId = parkingLotId, Odometer = odometer });

    public async Task<bool> UpdateVehicleAsync(int id, string model, string plateNumber, int? parkingLotId, int odometer)
        => await PutAsync($"api/vehicles/{id}", new { Model = model, PlateNumber = plateNumber, CurrentParkingLotId = parkingLotId, Odometer = odometer });

    public async Task<bool> DeleteVehicleAsync(int id)
        => await DeleteAsync($"api/vehicles/{id}");

    public async Task<bool> BlockVehicleAsync(int id, string reason)
        => await PostBoolAsync($"api/vehicles/{id}/block", new { Reason = reason });

    public async Task<bool> UnblockVehicleAsync(int id)
        => await PostBoolAsync($"api/vehicles/{id}/unblock", null);

    public async Task<List<BlockLogItem>> GetVehicleBlockHistoryAsync(int vehicleId)
        => await GetListAsync<BlockLogItem>($"api/vehicles/{vehicleId}/blocks");

    public async Task<List<ParkingLotItem>> GetParkingLotsAsync()
        => await GetListAsync<ParkingLotItem>("api/parkinglots");

    public async Task<bool> CreateParkingLotAsync(string name, double latitude, double longitude, int capacity)
        => await PostBoolAsync("api/parkinglots", new { Name = name, Latitude = latitude, Longitude = longitude, TotalCapacity = capacity });

    public async Task<bool> UpdateParkingLotAsync(int id, string name, double latitude, double longitude, int capacity)
        => await PutAsync($"api/parkinglots/{id}", new { Name = name, Latitude = latitude, Longitude = longitude, TotalCapacity = capacity });

    public async Task<bool> DeleteParkingLotAsync(int id)
        => await DeleteAsync($"api/parkinglots/{id}");

    public async Task<PagedResult<BookingItem>?> GetBookingsAsync(int page = 1, int pageSize = 50, string sortBy = "starttime", string sortDir = "desc")
    {
        var response = await _client.GetAsync($"api/bookings?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDir={sortDir}");
        if (!response.IsSuccessStatusCode)
        {
            LastError = await ExtractErrorAsync(response);
            return null;
        }
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PagedResult<BookingItem>>(json, JsonOptions);
    }

    public async Task<StatisticsOverview?> GetStatisticsOverviewAsync()
    {
        var response = await _client.GetAsync("api/statistics/overview");
        if (!response.IsSuccessStatusCode)
        {
            LastError = await ExtractErrorAsync(response);
            return null;
        }
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<StatisticsOverview>(json, JsonOptions);
    }

    private async Task<List<T>> GetListAsync<T>(string url)
    {
        var response = await _client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            LastError = await ExtractErrorAsync(response);
            return [];
        }
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? [];
    }

    private async Task<bool> PostBoolAsync(string url, object? body)
    {
        var response = body is null
            ? await _client.PostAsync(url, null)
            : await _client.PostAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode)
        {
            LastError = await ExtractErrorAsync(response);
            return false;
        }
        LastError = null;
        return true;
    }

    private async Task<bool> PutAsync(string url, object body)
    {
        var response = await _client.PutAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode)
        {
            LastError = await ExtractErrorAsync(response);
            return false;
        }
        LastError = null;
        return true;
    }

    private async Task<bool> DeleteAsync(string url)
    {
        var response = await _client.DeleteAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            LastError = await ExtractErrorAsync(response);
            return false;
        }
        LastError = null;
        return true;
    }

    private static async Task<string?> ExtractErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            if (doc.TryGetProperty("Message", out var msg) || doc.TryGetProperty("message", out msg))
                return msg.GetString();
        }
        catch { }
        return $"HTTP {(int)response.StatusCode}";
    }
}
