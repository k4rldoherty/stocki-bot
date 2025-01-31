using Discord.WebSocket;
using Discord;
using DiscordBot.Services.Utils;

namespace DiscordBot.Services;

public class SlashCommandsService(ApiService apiService, BotResponsesService botResponsesService)
{
    public async Task InitializeCommandsAsync(DiscordSocketClient client)
    {
        var globalCommands = await client.GetGlobalApplicationCommandsAsync();

        // TODO: Refactor this as file will get large with many commands
        if (!globalCommands.Any(x => x.Name.Equals("stock-price")))
        {
            var userCommand = new SlashCommandBuilder()
                .WithName("stock-price")
                .WithDescription("Gets the price of a stock by passing in a ticker eg 'PLTR' after the command")
                .AddOption("ticker", ApplicationCommandOptionType.String, "The symbol to get the price of a stock",
                    true)
                .Build();

            await client.CreateGlobalApplicationCommandAsync(userCommand);
        }

        else if (!globalCommands.Any(x => x.Name == "info"))
        {
            var userCommand = new SlashCommandBuilder()
                .WithName("info")
                .WithDescription("Retrives information about a stock by passing in a ticker eg 'PLTR'")
                .AddOption("ticker", ApplicationCommandOptionType.String, "The ticker symbol of a stock", true)
                .Build();

            await client.CreateGlobalApplicationCommandAsync(userCommand);
        }

        // More commands here ...
    }

    public async Task<string> HandleGetPriceAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetSingleStockPriceDailyDataAsync(ticker);
        if (response.Data is null) return botResponsesService.ErrorResponse(ticker, response.Message);
        var data = response.Data[0];
        var dailyTimeSeries = data.RootElement
            .GetProperty("Time Series (Daily)")
            .EnumerateObject();
        dailyTimeSeries.MoveNext();
        if (!decimal.TryParse(dailyTimeSeries.Current.Value.GetProperty("4. close").GetString(), out var priceCurrent))
            return response.Message;
        if (!dailyTimeSeries.MoveNext()) botResponsesService.ErrorResponse(ticker, response.Message);
        return !decimal.TryParse(dailyTimeSeries.Current.Value.GetProperty("4. close").GetString(), out var priceNext)
            ? botResponsesService.ErrorResponse(ticker, response.Message)
            : botResponsesService.FormatSingleStockDaily(ticker, priceCurrent, priceNext);
    }
}