using Discord;
using DiscordBot.Data;
using DiscordBot.Data.Models;

namespace DiscordBot.Services;

public class SubscriptionService
{
    public Dictionary<ulong, StockNotificationSubscription> subscriptionsInProgress;
    private readonly ApiService _apiService;
    private readonly SubscriptionRepository _subscriptionRepository;

    public SubscriptionService(ApiService apiService, SubscriptionRepository subscriptionRepository)
    {
        _apiService = apiService;
        _subscriptionRepository = subscriptionRepository;
        subscriptionsInProgress = new Dictionary<ulong, StockNotificationSubscription>();
    }

    // Check to see if stock exists.
    public async Task<bool> CheckValidTickerAsync(string ticker)
    {
        ticker = ticker.ToUpper();
        var response = await _apiService.GetSingleStockPriceDailyDataAsync(ticker);
        if (response.Data[0] is null)
            return false;
        return true;
    }

    // Make a popup that asks the user what kind of notifications they want.
    public MessageComponent SpawnNotificationSelectMenu()
    {
        var notificationSelectionMenu = new SelectMenuBuilder()
            .WithPlaceholder("Choose the notification types you want to recieve from")
            .WithCustomId("SubscriptionNotificationTypeMenu")
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Messages", "1", "Message alerts to you personally from Stocki!")
            .AddOption(
                "E-Mails",
                "2",
                "Email notifications (You will need to provide your email address.)"
            )
            .AddOption(
                "Price Changes",
                "3",
                "Specifically subscribe to price action alerts through message, sent to you by Stocki!"
            )
            .AddOption("All", "4", "Subscribe to all Stocki's top notch alerts.");

        var builder = new ComponentBuilder().WithSelectMenu(notificationSelectionMenu);

        return builder.Build();
    }

    public async Task<bool> AddSubscriptionAsync(ulong userId)
    {
        if (subscriptionsInProgress[userId] is null)
            return false;

        // TODO: More validation etc to be done here.
        if (
            _subscriptionRepository.GetSingleSubscription(
                userId,
                subscriptionsInProgress[userId].Ticker
            )
            is not null
        )
            return false;
        var sub = subscriptionsInProgress[userId];
        subscriptionsInProgress.Remove(userId);
        return await _subscriptionRepository.AddSubscriptionAsync(sub);
    }

    // If the user wants email, provide the user with a text box that they can enter their email.
    // public MessageComponent SpawnEmailTextBox(string email)
    // {
    //     var emailTextBox = new TextInputBuilder()
    //         .WithLabel("Your E-Mail")
    //         .WithCustomId("EmailTextBox")
    //         .WithRequired(true);
    //
    //
    // }
}
