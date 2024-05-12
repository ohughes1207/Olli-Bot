
// Temporarily commented out as this command broke when refactoring Bot class to not have static Client, Config and Slash properties

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using OlliBot;

namespace self_bot.modules.commands
{
    public class SetCommands : ApplicationCommandModule
    {        
        [SlashCommand("setchannel", "Set the default channel for the bot")]
        public static async Task SetChannel(InteractionContext ctx, [Option("channel", "Channel"), ChannelTypes(0)] DiscordChannel channel)
        {
            ulong Id = channel.Id;
            Bot.Config.BotChannel = Id;

            await Bot.SaveConfig();

            var embed = new DiscordEmbedBuilder();
            embed.WithDescription($"-----------------BEEP------------------\n-----------------BOOP-----------------\nChannel has been set to {channel.Mention}");
            await ctx.CreateResponseAsync(embed.Build());
        }
    }
}