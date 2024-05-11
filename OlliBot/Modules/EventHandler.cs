using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace OlliBot.Modules
{
    public class EventHandler
    {
        public static async Task OnMessage(DiscordClient Client, MessageCreateEventArgs args)
        {
            if (args.Message.Content.Length > 500)
            {
                await Client.SendMessageAsync(args.Channel, new DiscordMessageBuilder().WithReply(args.Message.Id).WithContent("i ain't reading all that"));
                await Client.SendMessageAsync(args.Channel, new DiscordMessageBuilder().WithContent("i'm happy for u tho"));
                await Client.SendMessageAsync(args.Channel, new DiscordMessageBuilder().WithContent("or sorry that happened"));
                return;
            }

            
            // Add more logic to this event
        }
    }
}