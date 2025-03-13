using Discord.WebSocket;

namespace DiscordBot.Services;

public class SlashCommandHandler
{
    private readonly SlashCommandsService _slashCommands;
    private readonly SubscriptionService _subscriptionService;

    public SlashCommandHandler(
        SlashCommandsService slashCommands,
        SubscriptionService subscriptionService
    )
    {
        _slashCommands = slashCommands;
        _subscriptionService = subscriptionService;
    }

    public async Task HandleCommandAsync(SocketSlashCommand command)
    {
        switch (command.CommandName)
        {
            case "stock-price":
                var stockPriceArg = command
                    .Data.Options.First(x => x.Name.Equals("ticker"))
                    .Value.ToString();
                if (stockPriceArg is not null)
                {
                    var response = await _slashCommands.HandleGetPriceAsync(stockPriceArg);
                    await command.RespondAsync(response);
                }
                break;

            case "info":
                var infoArg = command
                    .Data.Options.First(x => x.Name.Equals("ticker"))
                    .Value.ToString();
                if (infoArg is not null)
                {
                    var response = await _slashCommands.HandleGetInfoAsync(infoArg);
                    await command.RespondAsync(response);
                }
                break;

            case "latest-news":
                var companyNewsArg = command
                    .Data.Options.First(x => x.Name.Equals("ticker"))
                    .Value.ToString();
                if (companyNewsArg is not null)
                {
                    var response = await _slashCommands.HandleGetCompanyNewsAsync(companyNewsArg);
                    await command.RespondAsync(response);
                }
                break;

            case "subscribe":
                var subscriptionArg = command
                    .Data.Options.First(x => x.Name.Equals("ticker"))
                    .Value.ToString();
                if (subscriptionArg is not null)
                {
                    var subscriptionStarted = await _slashCommands.HandleSubscribeAsync(
                        subscriptionArg,
                        command.User.Id
                    );
                    if (!subscriptionStarted)
                    {
                        await command.RespondAsync(
                            "Something went wrong starting the subscription process. Please check your ticker symbol and try again."
                        );
                        return;
                    }
                    var selectMenu = _subscriptionService.SpawnNotificationSelectMenu();
                    await command.RespondAsync(
                        "What type of commands would you like to recieve",
                        components: selectMenu
                    );
                }
                break;

            default:
                await command.RespondAsync("Unknown command.");
                break;
        }
    }
}
