using Discord;
using Discord.WebSocket;
using DiscordBot.Core;
using DiscordBot.Data.Models;

namespace DiscordBot.Services;

public class SlashCommandsService(ApiService apiService, SubscriptionService subscriptionService)
{
    public static async Task InitializeCommandsAsync(DiscordSocketClient client)
    {
        var globalCommands = await client.GetGlobalApplicationCommandsAsync();

        // TODO: Refactor this as file will get large with many commands
        if (!globalCommands.Any(x => x.Name.Equals("stock-price")))
        {
            var priceCommand = new SlashCommandBuilder()
                .WithName("stock-price")
                .WithDescription(
                    "Gets the price of a stock by passing in a ticker eg 'PLTR' after the command"
                )
                .AddOption(
                    "ticker",
                    ApplicationCommandOptionType.String,
                    "The symbol to get the price of a stock",
                    true
                )
                .Build();

            await client.CreateGlobalApplicationCommandAsync(priceCommand);
        }
        else if (!globalCommands.Any(x => x.Name == "info"))
        {
            var infoCommand = new SlashCommandBuilder()
                .WithName("info")
                .WithDescription(
                    "Retrives information about a stock by passing in a ticker eg 'PLTR'"
                )
                .AddOption(
                    "ticker",
                    ApplicationCommandOptionType.String,
                    "The ticker symbol of a stock",
                    true
                )
                .Build();

            await client.CreateGlobalApplicationCommandAsync(infoCommand);
        }
        else if (!globalCommands.Any(x => x.Name == "subscribe"))
        {
            var subscribeCommand = new SlashCommandBuilder()
                .WithName("subscribe")
                .WithDescription(
                    "Subscribes the user to price changes and latest news in a stock, either by email, message or both."
                )
                .AddOption(
                    "ticker",
                    ApplicationCommandOptionType.String,
                    "The ticker symbol of the stock you want to subscribe to notifications about",
                    true
                )
                .Build();
            await client.CreateGlobalApplicationCommandAsync(subscribeCommand);
        }
        else if (!globalCommands.Any(x => x.Name == "unsubscribe"))
        {
            var unsubscribeCommand = new SlashCommandBuilder()
                .WithName("unsubscribe")
                .WithDescription(
                    "Unsubscribes the user to price changes and latest news in a stock, either by email, message or both."
                )
                .AddOption(
                    "ticker",
                    ApplicationCommandOptionType.String,
                    "The ticker symbol of the stock you want to unsubscribe to notifications about",
                    true
                )
                .Build();
            await client.CreateGlobalApplicationCommandAsync(unsubscribeCommand);
        }
        // More commands here ...
    }

    public async Task<OperationResponse> HandleSubscribeAsync(string ticker, ulong userId)
    {
        ticker = ticker.ToUpper();
        if (subscriptionService.subscriptionsInProgress.TryGetValue(userId, out var _))
        {
            return new OperationResponse(
                false,
                $"You already have a subscription in progress. Please answer the form already open."
            );
        }
        if (subscriptionService.IsUserAlreadySubscribed(userId, ticker))
        {
            return new OperationResponse(false, $"You are already subscribed to {ticker}.");
        }
        if (!await subscriptionService.CheckValidTickerAsync(ticker))
        {
            return new OperationResponse(
                false,
                "This ticker is not supported by our system. Please check the ticker and try again."
            );
        }
        var subcription = new StockNotificationSubscription
        {
            DiscordUID = userId,
            NotificationType = 0,
            Ticker = ticker,
            CreatedAt = DateTime.Now.ToUniversalTime(),
            LastNotification = DateTime.Now.ToUniversalTime(),
            IsActive = false,
        };
        subscriptionService.subscriptionsInProgress.Add(userId, subcription);
        return new OperationResponse(true, "Subscription process started");
    }

    public async Task HandleUnsubscribeAsync(string ticker)
    {
        await Task.CompletedTask;
    }

    public async Task<string> HandleGetPriceOnlyAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetStockPriceDataAsync(ticker);
        if (response.Data is null)
            return BotResponsesService.ErrorResponse(response.Message);
        var data = response.Data[0];
        var dailyTimeSeries = data
            .RootElement.GetProperty("Time Series (Daily)")
            .EnumerateObject()
            .First();
        if (!dailyTimeSeries.Value.TryGetProperty("4. close", out var priceCurrent))
            return response.Message;
        return priceCurrent.ToString();
    }

    // TODO: Clean this up, it was late at night i wanted to go to bed lol
    public async Task<string> HandleGetPriceAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetStockPriceDataAsync(ticker);
        if (response.Data is null)
            return BotResponsesService.ErrorResponse(response.Message);
        var data = response.Data[0].RootElement;
        if (!data.TryGetProperty("c", out var current))
        {
            return BotResponsesService.ErrorResponse(
                "[HandleGetPriceAsync]: Unable to parse current price"
            );
        }
        if (!data.TryGetProperty("d", out var change))
        {
            return BotResponsesService.ErrorResponse(
                "[HandleGetPriceAsync]: Unable to parse change"
            );
        }
        if (change.ValueKind == System.Text.Json.JsonValueKind.Null)
        {
            return BotResponsesService.ErrorResponse("[HandleGetPriceAsync]: Ticker not found");
        }
        if (!data.TryGetProperty("dp", out var percentageChange))
        {
            return BotResponsesService.ErrorResponse(
                "[HandleGetPriceAsync]: Unable to parse % change"
            );
        }
        if (!data.TryGetProperty("o", out var open))
        {
            return BotResponsesService.ErrorResponse(
                "[HandleGetPriceAsync]: Unable to parse opening price"
            );
        }
        if (!data.TryGetProperty("h", out var high))
        {
            return BotResponsesService.ErrorResponse(
                "[HandleGetPriceAsync]: Unable to parse high price"
            );
        }
        if (!data.TryGetProperty("l", out var low))
        {
            return BotResponsesService.ErrorResponse(
                "[HandleGetPriceAsync]: Unable to parse low price"
            );
        }

        return BotResponsesService.FormatSingleStockDaily(
            ticker,
            Convert.ToDecimal(current.ToString()),
            Convert.ToDecimal(change.ToString()),
            Convert.ToDecimal(percentageChange.ToString()),
            Convert.ToDecimal(open.ToString()),
            Convert.ToDecimal(high.ToString()),
            Convert.ToDecimal(low.ToString())
        );
    }

    public async Task<string> HandleGetInfoAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetStockInfoAsync(ticker);
        if (response.Data is null)
            return BotResponsesService.ErrorResponse(response.Message);
        var data = response.Data[0].RootElement;
        if (!data.TryGetProperty("Symbol", out var symbol))
            return BotResponsesService.ErrorResponse(
                "[HandleGetInfoAsync]: Unable to parse symbol"
            );
        if (!data.TryGetProperty("Name", out var name))
            return BotResponsesService.ErrorResponse("[HandleGetInfoAsync]: Unable to parse name");
        if (!data.TryGetProperty("Description", out var desc))
            return BotResponsesService.ErrorResponse(
                "[HandleGetInfoAsync]: Unable to parse description"
            );
        if (!data.TryGetProperty("Sector", out var sector))
            return BotResponsesService.ErrorResponse(
                "[HandleGetInfoAsync]: Unable to parse sector"
            );
        if (!data.TryGetProperty("EPS", out var eps))
            return BotResponsesService.ErrorResponse(
                "[HandleGetInfoAsync]: Unable to parse symbol"
            );
        if (!data.TryGetProperty("AnalystTargetPrice", out var analystPriceTarget))
            return BotResponsesService.ErrorResponse(
                "[HandleGetInfoAsync]: Unable to parse analyst price targets"
            );
        var stockDetails = new StockSummary(
            symbol.ToString(),
            name.ToString(),
            desc.ToString(),
            sector.ToString(),
            Decimal.Parse(await HandleGetPriceOnlyAsync(ticker)),
            Decimal.Parse(eps.ToString()),
            Decimal.Parse(analystPriceTarget.ToString())
        );

        return BotResponsesService.FormatStockSummary(stockDetails);
    }

    public async Task<string> HandleGetCompanyNewsAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetCompanyNewsAsync(ticker);
        if (response.Data is null)
            return BotResponsesService.ErrorResponse(response.Message);
        var data = response.Data[0].RootElement.EnumerateArray().Take(3).ToList();
        var articles = new List<CompanyNewsArticle>();
        foreach (var item in data)
        {
            if (!item.TryGetProperty("datetime", out var datetime))
                return BotResponsesService.ErrorResponse(
                    "[HandleGetCompanyNewsAsync]: Unable to parse datetime"
                );
            if (!item.TryGetProperty("headline", out var headline))
                return BotResponsesService.ErrorResponse(
                    "[HandleGetCompanyNewsAsync]: Unable to parse headline"
                );
            if (!item.TryGetProperty("source", out var source))
                return BotResponsesService.ErrorResponse(
                    "[HandleGetCompanyNewsAsync]: Unable to parse source"
                );
            if (!item.TryGetProperty("summary", out var summary))
                return BotResponsesService.ErrorResponse(
                    "[HandleGetCompanyNewsAsync]: Unable to parse summary"
                );
            if (!item.TryGetProperty("url", out var url))
                return BotResponsesService.ErrorResponse(
                    "[HandleGetCompanyNewsAsync]: Unable to parse url"
                );
            if (!item.TryGetProperty("related", out var related))
                return BotResponsesService.ErrorResponse(
                    "[HandleGetCompanyNewsAsync]: Unable to parse related"
                );
            if (!item.TryGetProperty("image", out var image))
                return BotResponsesService.ErrorResponse(
                    "[HandleGetCompanyNewsAsync]: Unable to parse image"
                );
            articles.Add(
                new CompanyNewsArticle(
                    Double.Parse(datetime.ToString()),
                    headline.ToString(),
                    source.ToString(),
                    summary.ToString(),
                    url.ToString(),
                    related.ToString(),
                    image.ToString()
                )
            );
        }
        return BotResponsesService.FormatCompanyNews(articles);
    }
}
