using DiscordBot.Services.Utils;

namespace DiscordBot.Services;

public class SlashCommandsService(ApiService apiService, BotResponsesService botResponsesService)
{
    public async Task<string> HandleGetPriceAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetSingleStockPriceDailyDataAsync(ticker);
        if (response.Data is null) return response.Message;
        var data = response.Data[0];
        var dailyTimeSeries = data.RootElement
            .GetProperty("Time Series (Daily)")
            .EnumerateObject();
        dailyTimeSeries.MoveNext();
        if (!decimal.TryParse(dailyTimeSeries.Current.Value.GetProperty("4. close").GetString(), out var priceCurrent))
            return response.Message;
        if (!dailyTimeSeries.MoveNext()) return response.Message;
        return !decimal.TryParse(dailyTimeSeries.Current.Value.GetProperty("4. close").GetString(), out var priceNext)
            ? response.Message
            : botResponsesService.FormatSingleStockDaily(ticker, priceCurrent, priceNext);
    }
}