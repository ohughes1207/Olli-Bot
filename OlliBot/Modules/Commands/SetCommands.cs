/*using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

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
}*/