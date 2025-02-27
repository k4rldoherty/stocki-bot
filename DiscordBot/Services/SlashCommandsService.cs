using Discord.WebSocket;
using Discord;
using DiscordBot.Core;

namespace DiscordBot.Services;

public class SlashCommandsService(ApiService apiService)
{
    public static async Task InitializeCommandsAsync(DiscordSocketClient client)
    {
        var globalCommands = await client.GetGlobalApplicationCommandsAsync();

        // TODO: Refactor this as file will get large with many commands
        if (!globalCommands.Any(x => x.Name.Equals("stock-price")))
        {
            var priceCommand = new SlashCommandBuilder()
                .WithName("stock-price")
                .WithDescription("Gets the price of a stock by passing in a ticker eg 'PLTR' after the command")
                .AddOption("ticker", ApplicationCommandOptionType.String, "The symbol to get the price of a stock",
                    true)
                .Build();

            await client.CreateGlobalApplicationCommandAsync(priceCommand);
        }

        else if (!globalCommands.Any(x => x.Name == "info"))
        {
            var infoCommand = new SlashCommandBuilder()
                .WithName("info")
                .WithDescription("Retrives information about a stock by passing in a ticker eg 'PLTR'")
                .AddOption("ticker", ApplicationCommandOptionType.String, "The ticker symbol of a stock", true)
                .Build();

            await client.CreateGlobalApplicationCommandAsync(infoCommand);
        }

        // More commands here ...
    }

    public async Task<string> HandleGetPriceOnlyAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetSingleStockPriceDailyDataAsync(ticker);
        if (response.Data is null) return BotResponsesService.ErrorResponse(response.Message);
        var data = response.Data[0];
        var dailyTimeSeries = data.RootElement
            .GetProperty("Time Series (Daily)")
            .EnumerateObject()
            .First();
        if (!dailyTimeSeries.Value.TryGetProperty("4. close", out var priceCurrent)) return response.Message;
        return priceCurrent.ToString();
    }

    public async Task<string> HandleGetPriceAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetSingleStockPriceDailyDataAsync(ticker);
        if (response.Data is null) return BotResponsesService.ErrorResponse(response.Message);
        var data = response.Data[0];
        var dailyTimeSeries = data.RootElement
            .GetProperty("Time Series (Daily)")
            .EnumerateObject();
        dailyTimeSeries.MoveNext();
        if (!decimal.TryParse(dailyTimeSeries.Current.Value.GetProperty("4. close").GetString(), out var priceCurrent))
            return response.Message;
        if (!dailyTimeSeries.MoveNext()) BotResponsesService.ErrorResponse(response.Message);
        return !decimal.TryParse(dailyTimeSeries.Current.Value.GetProperty("4. close").GetString(), out var priceNext)
            ? BotResponsesService.ErrorResponse(response.Message)
            : BotResponsesService.FormatSingleStockDaily(ticker, priceCurrent, priceNext);
    }

    public async Task<string> HandleGetInfoAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await apiService.GetStockInfoAsync(ticker);
        if (response.Data is null) return BotResponsesService.ErrorResponse(response.Message);
        var data = response.Data[0].RootElement;
        if (!data.TryGetProperty("Symbol", out var symbol)) return BotResponsesService.ErrorResponse("[HandleGetInfoAsync]: Unable to parse symbol");
        if (!data.TryGetProperty("Name", out var name)) return BotResponsesService.ErrorResponse("[HandleGetInfoAsync]: Unable to parse name");
        if (!data.TryGetProperty("Description", out var desc)) return BotResponsesService.ErrorResponse("[HandleGetInfoAsync]: Unable to parse description");
        if (!data.TryGetProperty("Sector", out var sector)) return BotResponsesService.ErrorResponse("[HandleGetInfoAsync]: Unable to parse sector");
        if (!data.TryGetProperty("EPS", out var eps)) return BotResponsesService.ErrorResponse("[HandleGetInfoAsync]: Unable to parse symbol");
        if (!data.TryGetProperty("AnalystTargetPrice", out var analystPriceTarget)) return BotResponsesService.ErrorResponse("[HandleGetInfoAsync]: Unable to parse analyst price targets");
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
        if (response.Data is null) return BotResponsesService.ErrorResponse(response.Message);
        var data = response.Data[0].RootElement.EnumerateArray().Take(3).ToList();
        var articles = new List<CompanyNewsArticle>();
        foreach (var item in data)
        {
            if (!item.TryGetProperty("datetime", out var datetime)) return BotResponsesService.ErrorResponse("[HandleGetCompanyNewsAsync]: Unable to parse datetime");
            if (!item.TryGetProperty("headline", out var headline)) return BotResponsesService.ErrorResponse("[HandleGetCompanyNewsAsync]: Unable to parse headline");
            if (!item.TryGetProperty("source", out var source)) return BotResponsesService.ErrorResponse("[HandleGetCompanyNewsAsync]: Unable to parse source");
            if (!item.TryGetProperty("summary", out var summary)) return BotResponsesService.ErrorResponse("[HandleGetCompanyNewsAsync]: Unable to parse summary");
            if (!item.TryGetProperty("url", out var url)) return BotResponsesService.ErrorResponse("[HandleGetCompanyNewsAsync]: Unable to parse url");
            if (!item.TryGetProperty("related", out var related)) return BotResponsesService.ErrorResponse("[HandleGetCompanyNewsAsync]: Unable to parse related");
            if (!item.TryGetProperty("image", out var image)) return BotResponsesService.ErrorResponse("[HandleGetCompanyNewsAsync]: Unable to parse image");
            articles.Add(new CompanyNewsArticle(Double.Parse(datetime.ToString()), headline.ToString(), source.ToString(), summary.ToString(), url.ToString(), related.ToString(), image.ToString()));
        }
        return BotResponsesService.FormatCompanyNews(articles);
    }
}
