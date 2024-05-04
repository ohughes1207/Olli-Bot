using DSharpPlus;
using DSharpPlus.EventArgs;

namespace self_bot.modules
{
    public class BotInitialization
    {
        public static async Task InitializationTasks(DiscordClient Client, ReadyEventArgs args)
        {
            Client.MessageCreated += EventHandler.OnMessage;
            await Task.CompletedTask;
        }
    }
}