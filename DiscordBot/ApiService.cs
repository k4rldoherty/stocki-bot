using System.Net;
using System.Text.Json;
using DiscordBot.Models;

namespace DiscordBot;

public class ApiService(HttpClient httpClient)
{
    // TODO: Env variable
    private const string ApiToken = "123.ie";

    public async Task<ApiResponse> GetSingleStockPriceDailyDataAsync(string ticker)
    {
        var url =
            $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={ticker}&apikey={ApiToken}";
        var response = await httpClient.GetAsync(url);
        if (response.StatusCode != HttpStatusCode.OK) return new ApiResponse("Something went wrong", null);
        var responseStr = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseStr);
        return new ApiResponse($"{response.StatusCode}", [jsonDoc]);
    }
}