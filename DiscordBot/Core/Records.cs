namespace DiscordBot;

public record WebSocketParsedMessage(decimal price, string ticker, long timestamp);

public record StockSummary(
    string symbol,
    string name,
    string description,
    string sector,
    decimal price,
    decimal eps,
    decimal priceTarget
);
