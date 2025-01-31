using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot.Services;
using DiscordBot.Services.Utils;

namespace DiscordBot;

public static class Program
{
    private static ServiceProvider ConfigureServices()
    {
        // Different way of doing it than i'm used to.
        var services = new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<SlashCommandsService>()
            .AddSingleton<ApiService>()
            .AddSingleton<HttpClient>()
            .AddSingleton<BotResponsesService>()
            .BuildServiceProvider();

        return services;
    }

    private static async Task MessageHandler(SocketMessage msg)
    {
        // Stops the bot from replying to itself
        if (msg.Author.IsBot) return;
        // TODO: some text functionality can be implemented here .. openAI API or something.
        // Reply to all dm messages or any time it is mentioned.
        if (msg.Channel.ChannelType == ChannelType.DM || msg.MentionedUsers.Any(x => x.IsBot))
        {
            await msg.Channel.SendMessageAsync(
                $"Hello {msg.Author.Username}!\nI am currently a work in progress and don't have the brain power to converse with you.\nCheck back soon...");
        }
    }

    private static async Task InitializeCommandsAsync(DiscordSocketClient client)
    {
        var globalCommands = await client.GetGlobalApplicationCommandsAsync();

        // TODO: Refactor this as file will get large with many commands
        if (!globalCommands.Any(x => x.Name.Equals("stock-price")))
        {
            var userCommand = new SlashCommandBuilder()
                .WithName("stock-price")
                .WithDescription("Gets the price of a stock by passing in a ticker eg 'PLTR' after the command")
                .AddOption("ticker", ApplicationCommandOptionType.String, "The symbol to get the price of a stock",
                    true)
                .Build();

            await client.CreateGlobalApplicationCommandAsync(userCommand);
        }

        // More commands here ...
    }

    public static async Task Main(string[] args)
    {
        // Configure services used in bot
        var services = ConfigureServices();
        var client = services.GetRequiredService<DiscordSocketClient>();
        var slashCommands = services.GetRequiredService<SlashCommandsService>();
        services.GetRequiredService<LoggingService>();
        // TODO: Change to env variable
        var token = "123.ie";

        // Login and start the bot
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Handle Messages
        client.MessageReceived += MessageHandler;

        // Initialize commands
        client.Ready += async () => await InitializeCommandsAsync(client);

        // Slash command executed
        client.SlashCommandExecuted += async (command) =>
        {
            switch (command.CommandName)
            {
                case "stock-price":
                    var ticker = command.Data.Options.First(x => x.Name.Equals("ticker")).Value.ToString();
                    if (ticker != null)
                    {
                        var response = await slashCommands.HandleGetPriceAsync(ticker);
                        await command.RespondAsync(response);
                    }

                    break;
            }
        };

        // Keeps the bot running
        await Task.Delay(-1);
    }
}