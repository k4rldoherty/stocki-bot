using System.Net;
using System.Text.Json;
using DiscordBot.Core;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string apiKey1;
    private readonly string apiKey2;

    public ApiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        this.apiKey1 =
            config["ALPHA_VANTAGE_API_KEY"]
            ?? throw new NullReferenceException("ALPHA VANTAGE API KEY NOT FOUND");
        this.apiKey2 =
            config["FINNHUB_API_KEY"]
            ?? throw new NullReferenceException("FINNHUB API KEY NOT FOUND");
    }

    public async Task<ApiResponse> GetStockPriceDataAsync(string ticker)
    {
        var url = $"https://www.finnhub.io/api/v1/quote?token={apiKey2}&symbol={ticker}";
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return new ApiResponse(
                $"{response.StatusCode}: Something went wrong retrieving price data.",
                null
            );
        }
        var responseStr = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseStr);
        return new ApiResponse($"{response.StatusCode}", [jsonDoc]);
    }

    public async Task<ApiResponse> GetStockInfoAsync(string ticker)
    {
        var url =
            $"https://www.alphavantage.co/query?function=OVERVIEW&symbol={ticker}&apikey={apiKey1}";
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode != HttpStatusCode.OK)
            return new ApiResponse(
                $"{response.StatusCode}: Something went wrong when retrieving the data",
                null
            );
        var responseStr = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseStr);
        if (jsonDoc.RootElement.TryGetProperty("Information", out _))
            return new ApiResponse(
                "My owner is too cheap to pay for the premium API.. I'm going to sleep until tomorrow.",
                null
            );
        if (!jsonDoc.RootElement.TryGetProperty("Symbol", out _))
        {
            return new ApiResponse("API-E2", null);
        }
        return new ApiResponse($"{response.StatusCode}", [jsonDoc]);
    }

    public async Task<ApiResponse> GetCompanyNewsAsync(string ticker)
    {
        var url =
            $"https://www.finnhub.io/api/v1/company-news?token={apiKey2}&symbol={ticker}&from={DateTime.Now.AddDays(-7):yyyy-MM-dd}&to={DateTime.Now:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode != HttpStatusCode.OK)
            return new ApiResponse(
                $"{response.StatusCode}: Something went wrong retrieving the data",
                null
            );
        var responseStr = await response.Content.ReadAsStringAsync();
        if (responseStr == "[]")
            return new ApiResponse(
                $"Ticker symbol incorrect, please check your input and try again",
                null
            );
        var jsonDoc = JsonDocument.Parse(responseStr);
        return new ApiResponse($"{response.StatusCode}", [jsonDoc]);
    }

    public async Task<ApiResponse> CheckIsTickerValidAsync(string ticker)
    {
        var url = $"https://www.finnhub.io/api/v1/search?token={apiKey2}&q={ticker}&exchange=US";
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return new ApiResponse(
                $"{response.StatusCode}: Something went wrong retrieving the data",
                null
            );
        }
        var responseStr = await response.Content.ReadAsStringAsync();
        if (responseStr == "[]")
        {
            return new ApiResponse($"Ticker Symbol invalid, check input and try again", null);
        }
        var jsonDoc = JsonDocument.Parse(responseStr);
        return new ApiResponse($"{response.StatusCode}", [jsonDoc]);
    }
}
