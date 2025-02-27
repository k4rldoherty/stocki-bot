using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot;

public static class Startup 
{
  public static IServiceProvider ConfigureServices()
  {
    var config = new ConfigurationBuilder()
      .AddUserSecrets<Program>()
      .Build();

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
      .AddSingleton<BotService>()
      .AddSingleton<SlashCommandHandler>()
      .BuildServiceProvider();

    return services;
  }
}
