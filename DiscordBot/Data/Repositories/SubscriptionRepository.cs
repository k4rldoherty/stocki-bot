using DiscordBot.Data.Models;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data;

public class SubscriptionRepository(
    StockiContext stockiContext,
    ILogger<SubscriptionRepository> logger
)
{
    public async Task<bool> AddSubscriptionAsync(StockNotificationSubscription subscription)
    {
        await stockiContext.StockNotificationSubscriptions.AddAsync(subscription);
        var result = await stockiContext.SaveChangesAsync();
        if (result > 0)
        {
            logger.LogInformation("Subscription added successfully.");
            return true;
        }
        return false;
    }

    public StockNotificationSubscription? GetSingleSubscription(ulong id, string ticker)
    {
        var res = stockiContext
            .StockNotificationSubscriptions.Where(sns =>
                sns.DiscordUID == id && sns.Ticker == ticker && sns.IsActive
            )
            .FirstOrDefault();

        return res;
    }

    // TODO: Change this as could slow if lot of subscripions
    public List<StockNotificationSubscription>? GetAllSubscriptions()
    {
        var res = stockiContext.StockNotificationSubscriptions.Where(sns => sns.IsActive).ToList();
        return res;
    }
}
