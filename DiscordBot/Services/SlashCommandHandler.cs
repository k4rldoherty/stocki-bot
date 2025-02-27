using Discord.WebSocket;

namespace DiscordBot.Services;

public class SlashCommandHandler
{
    private readonly SlashCommandsService _slashCommands;

    public SlashCommandHandler(SlashCommandsService slashCommands)
    {
        _slashCommands = slashCommands;
    }

    public async Task HandleCommandAsync(SocketSlashCommand command)
    {
        switch (command.CommandName)
        {
            case "stock-price":
                var stockPriceArg = command.Data.Options.First(x => x.Name.Equals("ticker")).Value.ToString();
                if (stockPriceArg is not null)
                {
                    var response = await _slashCommands.HandleGetPriceAsync(stockPriceArg);
                    await command.RespondAsync(response);
                }
                break;

            case "info":
                var infoArg = command.Data.Options.First(x => x.Name.Equals("ticker")).Value.ToString();
                if (infoArg is not null)
                {
                    var response = await _slashCommands.HandleGetInfoAsync(infoArg);
                    await command.RespondAsync(response);
                }
                break;

            case "latest-news":
                var companyNewsArg = command.Data.Options.First(x => x.Name.Equals("ticker")).Value.ToString();
                if (companyNewsArg is not null)
                {
                    var response = await _slashCommands.HandleGetCompanyNewsAsync(companyNewsArg);
                    await command.RespondAsync(response);
                }
                break;

            default:
                await command.RespondAsync("Unknown command.");
                break;
        }
    }
}

