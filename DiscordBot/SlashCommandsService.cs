using System.Text.Json;
using DiscordBot.Utils;

namespace DiscordBot;

public class SlashCommandsService(ApiService apiService, BotResponsesService botResponsesService)
{
    public async Task<string> HandleGetPriceAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetSingleStockPriceDailyDataAsync(ticker);
        if (response.Data is null) return "Error Retrieving Price Data";
        var data = response.Data[0];
        var dailyTimeSeries = data.RootElement
            .GetProperty("Time Series (Daily)")
            .EnumerateObject();
        dailyTimeSeries.MoveNext();
        if (!decimal.TryParse(dailyTimeSeries.Current.Value.GetProperty("4. close").GetString(), out var priceCurrent))
            return "Error Retrieving Price Data";
        if (!dailyTimeSeries.MoveNext()) return "Error Retrieving Price Data";
        return !decimal.TryParse(dailyTimeSeries.Current.Value.GetProperty("4. close").GetString(), out var priceNext)
            ? "Error Retrieving Price Data"
            : botResponsesService.FormatSingleStockDaily(ticker, priceCurrent, priceNext);
    }
}