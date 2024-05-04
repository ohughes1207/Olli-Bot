/*using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using self_bot.modules.humblebundle;

namespace self_bot.modules.commands
{
    public class HumbleBundleCommands : ApplicationCommandModule
    {
        [SlashCommand("hb", "Get the latest humble bundle")]
        public static async Task GetHumbleBundleGames(InteractionContext ctx)
        {
            await HumbleBundleScraper.ScrapeHB();
        }

        [SlashCommand("hbset", "Set the channel for humblebundle updates")]
        public static async Task SetHBChannel(InteractionContext ctx, [Option("channel", "Channel"), ChannelTypes(0)] DiscordChannel channel)
        {

        }
    }
}
*/