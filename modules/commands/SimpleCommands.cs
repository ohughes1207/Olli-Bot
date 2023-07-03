using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace self_bot.modules.commands
{
    public class SimpleCommands : ApplicationCommandModule
    {        
        [SlashCommand("avatar", "Get the avatar of the specified user")]
        public static async Task Avatar(InteractionContext ctx, [Option("user", "Specified user")] DiscordUser user)
        {

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            var nickname = member.Nickname ?? member.DisplayName ?? user.Username;
            
            //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(user.AvatarUrl));
            var embed = new DiscordEmbedBuilder();
            embed.WithTitle($"{nickname}'s avatar");
            //embed.WithColor(DiscordColor.Orange);
            embed.WithColor(new DiscordColor(252, 177, 3));
            embed.WithUrl(user.AvatarUrl);
            embed.WithImageUrl(user.AvatarUrl);

            await ctx.CreateResponseAsync(embed.Build());
            //Console.Write(user);
        }
    }
}