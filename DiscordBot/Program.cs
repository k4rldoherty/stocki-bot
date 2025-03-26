using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Data;
using DiscordBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config =>
            {
                config
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddUserSecrets<Program>()
                    .AddEnvironmentVariables();
            })
            .ConfigureServices(
                (context, services) =>
                {
                    var config = context.Configuration;
                    // Discord .NET services
                    services.AddSingleton<DiscordSocketClient>();
                    services.AddSingleton<CommandService>();

                    // Services
                    services.AddSingleton<SlashCommandsService>();
                    services.AddSingleton<LoggingService>();
                    services.AddSingleton<BotResponsesService>();
                    services.AddSingleton<InputHandlerService>();
                    services.AddSingleton<SlashCommandHandler>();
                    services.AddTransient<ApiService>();
                    services.AddSingleton<SubscriptionService>();
                    services.AddSingleton<PriceActionService>();

                    // Hosted services - These are started automatically on starting the application
                    services.AddHostedService<WebsocketService>();
                    services.AddHostedService<BotService>();

                    // Repositories
                    services.AddSingleton<SubscriptionRepository>();

                    var connString = config["POSTGRES-CONNECTION-STRING"];

                    services.AddSingleton<HttpClient>();
                    services.AddDbContext<StockiContext>(options =>
                    {
                        options.UseNpgsql(connString);
                    });
                }
            )
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            })
            .Build();

        await host.RunAsync();
    }
}
