namespace DiscordBot.Core;

public class OperationResponse
{
    public bool Status { get; set; }
    public string Message { get; set; }

    public OperationResponse(bool s, string m)
    {
        Status = s;
        Message = m;
    }
}
