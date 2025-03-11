namespace DiscordBot.Data.Models;

public class StockNotificationSubscription
{
    public int Id { get; set; }
    public required string DiscordUID { get; set; }
    public required string Ticker { get; set; }
    public required NotificationType NotificationType { get; set; }
    public string? email { get; set; }
    public DateTime LastNotification { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public enum NotificationType
{
    Message = 1,
    Email = 2,
    PriceAction = 3,
    All = 4,
}
