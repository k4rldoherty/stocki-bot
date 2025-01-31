namespace DiscordBot.Services.Utils;

public class BotResponsesService
{
    // TODO: make this a component instead of just a string.
    public string FormatSingleStockDaily(string ticker, decimal curr, decimal prev)
    {
        var diff = (curr - prev) / curr * 100;
        return $"**{ticker}** is priced at **{curr}**\n\n**{ticker}'s** change compared to previous days closing: **{diff:#.##}%**";
    }
}