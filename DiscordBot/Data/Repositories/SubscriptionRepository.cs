using DiscordBot.Data.Models;

namespace DiscordBot.Data;

public class SubscriptionRepository(StockiContext stockiContext)
{
    public async Task<bool> AddSubscriptionAsync(StockNotificationSubscription subscription)
    {
        await stockiContext.StockNotificationSubscriptions.AddAsync(subscription);
        var result = await stockiContext.SaveChangesAsync();
        if (result > 0)
            return true;
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
}
