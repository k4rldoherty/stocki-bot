using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class BotService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly SlashCommandHandler _slashCommandHandler;
    private readonly InputHandlerService _inputHandler;
    private readonly IConfiguration _config;
    private readonly ILogger<BotService> _logger;

    // private readonly LoggingService _loggingService;

    public BotService(
        DiscordSocketClient client,
        SlashCommandHandler slashCommandHandler,
        InputHandlerService inputHandler,
        IConfiguration config,
        ILogger<BotService> logger
    // LoggingService loggingService
    )
    {
        _client = client;
        _slashCommandHandler = slashCommandHandler;
        _inputHandler = inputHandler;
        _config = config;
        _logger = logger;
        //    _loggingService = loggingService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = _config["DISCORD_TOKEN"];

        // Login and start the bot
        _logger.LogInformation("Attempting login...");
        await _client.LoginAsync(TokenType.Bot, token);
        _logger.LogInformation("Login successful...");
        await _client.StartAsync();

        // Handle Messages
        _client.MessageReceived += _inputHandler.HandleMessageAsync;

        // Handle Dropdown Menu - W.I.P
        _client.SelectMenuExecuted += _inputHandler.HandleSelectMenuAsync;

        // Initialize commands
        _client.Ready += async () => await SlashCommandsService.InitializeCommandsAsync(_client);

        // Handle slash commands
        _client.SlashCommandExecuted += _slashCommandHandler.HandleCommandAsync;

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning("Bot manually stopped. Shutting down");
            _logger.LogWarning(ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await _client.StopAsync();
        await base.StopAsync(stoppingToken);
    }
}
