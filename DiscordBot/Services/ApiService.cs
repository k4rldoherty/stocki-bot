using DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.Json;

namespace DiscordBot.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string apiKey;
    public ApiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        this.apiKey = config["ALPHA_VANTAGE_API_KEY"] ?? throw new NullReferenceException("API KEY NOT FOUND");

    }

    public async Task<ApiResponse> GetSingleStockPriceDailyDataAsync(string ticker)
    {
        var url =
            $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={ticker}&apikey={apiKey}";
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode != HttpStatusCode.OK) return new ApiResponse($"{response.StatusCode}: Something went wrong when retrieving the data", null);
        var responseStr = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseStr);
        if(!jsonDoc.RootElement.TryGetProperty("Meta Data", out var x)) 
        {
            return new ApiResponse("API-E1", null);
        }
        return new ApiResponse($"{response.StatusCode}", [jsonDoc]);
    }

    public async Task<ApiResponse> GetStockInfoAsync(string ticker)
    {
        var url =
            $"https://www.alphavantage.co/query?function=OVERVIEW&symbol={ticker}&apikey={apiKey}";
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode != HttpStatusCode.OK) return new ApiResponse($"{response.StatusCode}: Something went wrong when retrieving the data", null);
        var responseStr = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseStr);
        if(!jsonDoc.RootElement.TryGetProperty("Symbol", out var x))
        {
            return new ApiResponse("API-E2", null);
        }
        return new ApiResponse($"{response.StatusCode}", [jsonDoc]);
    }
}