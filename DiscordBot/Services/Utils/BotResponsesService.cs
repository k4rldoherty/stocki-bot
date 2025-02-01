using DiscordBot.Models;

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
            # Error Processing Request
            - I am having trouble carrying out this request
            - Check it was the correct ticker and try again, and if you think I am wrong you can contact my developer
            - Additionally, check the error message below as it may be helpful.
            
            *This bot currently only works on US based stocks, and the API can change without letting developers know so if you see any problems please report them!*
            
            ``` {message} ```

            Thanks,
            Stocki :)
            """;
    }

    public string FormatStockSummary(StockSummary stockSummary)
    {
        return $"""
            # {stockSummary.Name}
            ## Basic Info
            - Sector: **{stockSummary.Sector}**
            - Symbol: **{stockSummary.Symbol}**
            ## Description
            {stockSummary.Description}
            ## Financials
            ### Current Price: ${stockSummary.Price:#.##}
            - Earnings Per Share: **{stockSummary.Eps}**
            - Analyst Price Target (12 Month Horizon): **${stockSummary.AnalystPriceTarget}**

            *This feature will be continuously updated with more metrics, I'm just trying to get a working version out ASAP*
            """;
    }
}