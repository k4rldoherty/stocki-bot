namespace DiscordBot.Services.Utils;

public class BotResponsesService
{
    public string FormatSingleStockDaily(string ticker, decimal curr, decimal prev)
    {
        var diff = (curr - prev) / curr * 100;
        return $"""
            # {ticker}
            - **{ticker}** is priced at **{curr}**
            - The price of **{ticker}** is *{(diff>0?"up":"down")}* **{diff:#.##}%** compared to yesterday.
            - Use the command below to get more information on **{ticker}**
            ``` /info <ticker> ``` 
            {DateTime.Now}
            """;

    }

    public string ErrorResponse(string ticker, string message)
    {
        return $"""
            # Error
            - I can't seem to find a stock by the name **{ticker}**
            - Check it was the correct ticker and try again, and if you think I am wrong you can contact my developer
            *This bot currently only works on US based stocks.*
            ``` {message} ```
            """;
    }
}