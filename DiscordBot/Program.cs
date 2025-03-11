using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Data;
using DiscordBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

                    // My services
                    services.AddSingleton<SlashCommandsService>();
                    services.AddSingleton<LoggingService>();
                    services.AddSingleton<BotResponsesService>();
                    services.AddSingleton<MessageHandlerService>();
                    services.AddSingleton<BotService>();
                    services.AddSingleton<SlashCommandHandler>();
                    services.AddTransient<ApiService>();

                    var connString = config["POSTGRES-CONNECTION-STRING"];

                    services.AddSingleton<HttpClient>();
                    services.AddDbContext<StockiContext>(options =>
                    {
                        options.UseNpgsql(connString);
                    });
                }
            )
            .Build();

        var botService = host.Services.GetRequiredService<BotService>();

        await botService.StartAsync();
        await Task.Delay(-1); // Keeps bot running
    }
}
