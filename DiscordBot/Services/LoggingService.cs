using Discord;
using Discord.Commands;

namespace DiscordBot.Services;
using Discord.WebSocket;

public class LoggingService
{
    public LoggingService(DiscordSocketClient client, CommandService command)
    {
        client.Log += LogAsync;
        command.Log += LogAsync;
    }

    private static Task LogAsync(LogMessage log)
    {
        if (log.Exception is CommandException commandException)
        {
            Console.WriteLine($"[Command/{log.Severity}] {commandException.Command.Aliases[0]}"
                + $" failed to execute in {commandException.Context.Channel}.");
            Console.WriteLine(commandException);
        }
        else
        {
            Console.WriteLine($"[General/{log.Severity}] {log}");
        }
        return Task.CompletedTask;
    }
}