using Discord;
using Discord.WebSocket;
using DiscordBot.Data.Models;

namespace DiscordBot.Services
{
    public class InputHandlerService
    {
        private readonly SubscriptionService _subscriptionService;

        public InputHandlerService(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public async Task HandleMessageAsync(SocketMessage msg)
        {
            // Stops the bot from replying to itself
            if (msg.Author.IsBot)
                return;
            // TODO: some text functionality can be implemented here .. openAI API or something.
            // Reply to all dm messages or any time it is mentioned.
            if (
                msg.Channel.ChannelType.Equals(ChannelType.DM)
                || msg.MentionedUsers.Any(x => x.IsBot)
            )
            {
                msg.Channel.EnterTypingState();
                await msg.Channel.SendMessageAsync(
                    $"Hello {msg.Author.Username}!\nI am currently a work in progress and don't have the brain power to converse with you.\nCheck back soon..."
                );
            }
        }

        public async Task HandleSelectMenuAsync(SocketMessageComponent arg)
        {
            await arg.DeferAsync();
            if (arg.Data.CustomId == "SubscriptionNotificationTypeMenu")
            {
                string? selectedValue = arg.Data.Values.FirstOrDefault();
                if (selectedValue is not null && int.TryParse(selectedValue, out var val))
                {
                    var choice = (NotificationType)val;
                    if (_subscriptionService.subscriptionsInProgress[arg.User.Id] is null)
                    {
                        await arg.RespondAsync(
                            "Something went wrong processing your notification choice. Please try again"
                        );
                        return;
                    }
                    _subscriptionService.subscriptionsInProgress[arg.User.Id].NotificationType =
                        choice;
                    _subscriptionService.subscriptionsInProgress[arg.User.Id].IsActive = true;
                    var subscriptionAdded = await _subscriptionService.AddSubscriptionAsync(
                        arg.User.Id
                    );
                    if (!subscriptionAdded)
                    {
                        await arg.RespondAsync(
                            "Something went wrong adding your subscription to our db. Please try again (You may aleady have subscribed to this stock)."
                        );
                        return;
                    }
                    await arg.FollowupAsync(
                        $"""
                        Welcome to Stocki Notifications!
                        You choose to sign up for {choice.ToString().ToLower()} notifications for {_subscriptionService.subscriptionsInProgress[
                            arg.User.Id
                        ].Ticker}.
                        Processing your request and getting things ready...
                        """
                    );
                }
            }
        }
    }
}
