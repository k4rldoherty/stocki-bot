using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Services;

public class BotService
{
    private readonly DiscordSocketClient _client;
    private readonly SlashCommandHandler _slashCommandHandler;
    private readonly MessageHandlerService _messageHandler;
    private readonly IConfiguration _config;
    private readonly LoggingService _loggingService;

    public BotService(
        DiscordSocketClient client,
        SlashCommandHandler slashCommandHandler,
        MessageHandlerService messageHandler,
        IConfiguration config,
        LoggingService loggingService
    )
    {
        _client = client;
        _slashCommandHandler = slashCommandHandler;
        _messageHandler = messageHandler;
        _config = config;
        _loggingService = loggingService;
    }

    public async Task StartAsync()
    {
        var token = _config["DISCORD_TOKEN"];

        // Login and start the bot
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Handle Messages
        _client.MessageReceived += _messageHandler.HandleMessageAsync;

        // Initialize commands
        _client.Ready += async () => await SlashCommandsService.InitializeCommandsAsync(_client);

        // Handle slash commands
        _client.SlashCommandExecuted += _slashCommandHandler.HandleCommandAsync;
    }
}
