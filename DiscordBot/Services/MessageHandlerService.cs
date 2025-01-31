using Discord.WebSocket;
using Discord;

namespace DiscordBot.Services
{
    public class MessageHandlerService
    {
        public async Task HandleMessageAsync(SocketMessage msg)
        {
            // Stops the bot from replying to itself
            if (msg.Author.IsBot) return;
            // TODO: some text functionality can be implemented here .. openAI API or something.
            // Reply to all dm messages or any time it is mentioned.
            if (msg.Channel.ChannelType.Equals(ChannelType.DM) || msg.MentionedUsers.Any(x => x.IsBot))
            {
                msg.Channel.EnterTypingState();
                await msg.Channel.SendMessageAsync(
                    $"Hello {msg.Author.Username}!\nI am currently a work in progress and don't have the brain power to converse with you.\nCheck back soon...");
            }
        }
    }
}
