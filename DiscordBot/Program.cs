using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot;

public class Program
{
    public static async Task Main()
    {
        var services = Startup.ConfigureServices();
        var botService = services.GetRequiredService<BotService>();

        await botService.StartAsync();
        await Task.Delay(-1); // Keeps bot running
    }
}

