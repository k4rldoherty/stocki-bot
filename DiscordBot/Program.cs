using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot;

public class Program
{
    private static ServiceProvider ConfigureServices(IConfigurationRoot config)
    {
        // Different way of doing it than i'm used to.
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<SlashCommandsService>()
            .AddSingleton<ApiService>()
            .AddSingleton<HttpClient>()
            .AddSingleton<BotResponsesService>()
            .AddSingleton<MessageHandlerService>()
            .BuildServiceProvider();

        return services;
    }

    public static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        // Configure services used in bot
        var services = ConfigureServices(config);
        var client = services.GetRequiredService<DiscordSocketClient>();
        var slashCommands = services.GetRequiredService<SlashCommandsService>();
        var messageHandler = services.GetRequiredService<MessageHandlerService>();
        services.GetRequiredService<LoggingService>();
        var token = config["DISCORD_TOKEN"];

        // Login and start the bot
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Handle Messages
        client.MessageReceived += messageHandler.HandleMessageAsync;

        // Initialize commands
        client.Ready += async () => await SlashCommandsService.InitializeCommandsAsync(client);

        // Slash command executed
        client.SlashCommandExecuted += async (command) =>
        {
            switch (command.CommandName)
            {
                case "stock-price":
                    var stockPriceArg = command.Data.Options.First(x => x.Name.Equals("ticker")).Value.ToString();
                    if (stockPriceArg is not null)
                    {
                        var response = await slashCommands.HandleGetPriceAsync(stockPriceArg);
                        await command.RespondAsync(response);
                    }
                    break;

                case "info":
                    var infoArg = command.Data.Options.First(x => x.Name.Equals("ticker")).Value.ToString();
                    if (infoArg is not null)
                    {
                        var response = await slashCommands.HandleGetInfoAsync(infoArg);
                        await command.RespondAsync(response);
                    }
                    break;

                case "latest-news":
                    var companyNewsArg = command.Data.Options.First(x => x.Name.Equals("ticker")).Value.ToString();
                    if(companyNewsArg is not null)
                    {
                        var response = await slashCommands.HandleGetCompanyNewsAsync(companyNewsArg);
                        await command.RespondAsync(response);
                    }
                    break;

                default:
                    break;
            }
        };

        // Keeps the bot running
        await Task.Delay(-1);
    }
}