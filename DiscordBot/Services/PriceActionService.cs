using Microsoft.Extensions.Logging;

namespace DiscordBot;

public class PriceActionService
{
    private ILogger<PriceActionService> _logger;
    private Dictionary<string, decimal> prices;

    public PriceActionService(ILogger<PriceActionService> logger)
    {
        _logger = logger;
        prices = new();
    }

    public void CompareAndUpdatePrice(WebSocketParsedMessage message)
    {
        // If there is no last price, add it to the dict
        if (!prices.TryGetValue(message.ticker, out var prevPrice))
        {
            _logger.LogInformation($"New stock being added: {message.ticker}");
            prices.Add(message.ticker, message.price);
            return;
        }
        var priceChange = message.price - prevPrice;
        var fivePerCent = prevPrice / 20;
        if (priceChange > fivePerCent)
        {
            _logger.LogInformation(
                $"{message.ticker} has moved 5%. Notifying all subscribed users."
            );
        }
        else
        {
            _logger.LogInformation(
                $"No significant movement in {message.ticker}. Continuing. Price: {message.price}"
            );
        }
    }
}
