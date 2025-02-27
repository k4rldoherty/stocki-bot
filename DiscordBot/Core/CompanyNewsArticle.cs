

namespace DiscordBot.Core;

public class CompanyNewsArticle
{
  public string DateOfArticle;
  public string Headline;
  public string Source;
  public string Summary;
  public string Url;
  public string Related;
  public string ImageUrl;

  public CompanyNewsArticle(double timestamp, string headline, string source, string summary, string url, string related, string imageUrl)
  {
    this.DateOfArticle = UnixTimeStampToDateTime(timestamp);
    this.Headline = headline;
    this.Source = source;
    this.Summary = summary;
    this.Url = url;
    this.Related = related;
    this.ImageUrl = imageUrl;
  }

  private string UnixTimeStampToDateTime(double unixTime)
  {
    DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
    long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
    return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc).ToString("yyyy-MM-dd");
  }
}

