using System.Text.Json;

namespace DiscordBot.Core;

public class ApiResponse
{
    public string Message { get; set; }
    public JsonDocument[]? Data { get; set; }

    public ApiResponse(string m, JsonDocument[]? d)
    {
        Message = m;
        Data = d;
    }
}
