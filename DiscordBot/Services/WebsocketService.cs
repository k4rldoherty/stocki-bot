using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBot;

public class WebsocketService : BackgroundService
{
    private ILogger<WebsocketService> _logger;
    private ClientWebSocket _websocket;
    private Uri _uri;
    private List<String> _subscribedTickers = new() { "AAPL", "NVDA" };
    private readonly string _apiKey;
    private readonly TimeSpan _maxBackoff = TimeSpan.FromSeconds(30);

    public WebsocketService(ILogger<WebsocketService> logger, IConfiguration config)
    {
        _logger = logger;
        _apiKey =
            config["FINNHUB_API_KEY"]
            ?? throw new NullReferenceException("FINNHUB API KEY NOT FOUND");
        _uri = new($"wss://ws.finnhub.io?token={_apiKey}");
        _websocket = new ClientWebSocket();
    }

    // TODO: Grab the subdcribed tickers from the DB for connecting to websockets.
    private async Task GetSubscribedTickers()
    {
        await Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting websockets connection...");
        while (!stoppingToken.IsCancellationRequested)
        {
            await ConnectAndListenAsync(stoppingToken);
        }
    }

    private async Task ConnectAndListenAsync(CancellationToken stoppingToken)
    {
        var retryAttempt = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Connecting to socket");
                _websocket = new();
                await _websocket.ConnectAsync(_uri, stoppingToken);
                _logger.LogInformation("Connected, subscribing to tickers");
                await SubscribeTickersAsync();
                _logger.LogInformation("Tickers subscribed");
                await ListenForMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            // TODO: Look into exponential backoff.
            _logger.LogWarning($"Attempting reconnection in {_maxBackoff.TotalSeconds} seconds");
            await Task.Delay((int)_maxBackoff.TotalMilliseconds, stoppingToken);
            retryAttempt++;
        }
    }

    private async Task SubscribeTickersAsync()
    {
        foreach (var ticker in _subscribedTickers)
        {
            var message = JsonSerializer.Serialize(
                new { type = "subscribe", symbol = ticker.ToUpper() }
            );
            _logger.LogInformation($"Sending subscribe request: {message}");
            await _websocket.SendAsync(
                Encoding.UTF8.GetBytes(message),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
            _logger.LogInformation($"Websocket connection established with ticker {ticker}");
        }
    }

    private async Task ListenForMessagesAsync(CancellationToken stoppingToken)
    {
        byte[] buffer = new byte[1024 * 4];
        while (_websocket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
        {
            var result = await _websocket.ReceiveAsync(buffer, stoppingToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                _logger.LogWarning("Web socket connection closed.");
                await _websocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Websocket closed. Attempting to reconnect.",
                    stoppingToken
                );
                break;
            }
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _logger.LogInformation($"Recieved: {message}");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping websockets connection...");
        if (_websocket.State == WebSocketState.Open)
        {
            await _websocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "The websocket connection is now closed",
                stoppingToken
            );
        }
        _websocket.Dispose();
        await base.StopAsync(stoppingToken);
    }
}
