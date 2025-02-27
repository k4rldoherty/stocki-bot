using DiscordBot.Core;
using System.Text;

namespace DiscordBot.Services;

public class BotResponsesService
{
    public static string FormatSingleStockDaily(string ticker, decimal curr, decimal prev)
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

    public static string ErrorResponse(string message)
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

    public static string FormatStockSummary(StockSummary stockSummary)
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

    public static string FormatCompanyNews(List<CompanyNewsArticle> companyNewsArticles)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Top 3 news stories for {companyNewsArticles[0].Related}");
        foreach(var a in companyNewsArticles)
        {
            sb.Append($"""
                ## {a.Headline}
                > {a.DateOfArticle}
                > **Source**: {a.Source}
                {a.Summary}
                [Full article]({a.Url})
                """);
            sb.AppendLine();
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
