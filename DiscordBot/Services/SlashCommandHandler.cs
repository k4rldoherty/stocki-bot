using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class SlashCommandHandler
{
    private readonly SlashCommandsService _slashCommands;
    private readonly SubscriptionService _subscriptionService;
    private readonly ILogger<SlashCommandHandler> _logger;

    public SlashCommandHandler(
        SlashCommandsService slashCommands,
        SubscriptionService subscriptionService,
        ILogger<SlashCommandHandler> logger
    )
    {
        _slashCommands = slashCommands;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    public async Task HandleCommandAsync(SocketSlashCommand command)
    {
        _logger.LogInformation($"User {command.User.Username} executed /{command.CommandName}");
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
                _logger.LogInformation($"{command.CommandName} executed @ {DateTime.Now}\n");
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
                _logger.LogInformation($"{command.CommandName} executed @ {DateTime.Now}\n");
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
                _logger.LogInformation($"{command.CommandName} executed @ {DateTime.Now}\n");
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
                    if (!subscriptionStarted.Status)
                    {
                        await command.RespondAsync(subscriptionStarted.Message);
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
