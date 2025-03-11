using DiscordBot.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Data;

public class StockiContext : DbContext
{
    public StockiContext(DbContextOptions<StockiContext> options)
        : base(options) { }

    public DbSet<StockNotificationSubscription> StockNotificationSubscriptions { get; set; }
}
