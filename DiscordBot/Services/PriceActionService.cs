using DiscordBot.Data;
using Microsoft.Extensions.Logging;

namespace DiscordBot;

public class PriceActionService(
    ILogger<PriceActionService> logger,
    SubscriptionRepository subscriptionRepository
) { }
