using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace OlliBot.Modules
{
    public class BasicCommands : ApplicationCommandModule
    {        
        [SlashCommand("avatar", "Get the avatar of the specified user")]
        public static async Task Avatar(InteractionContext ctx, [Option("user", "Specified user")] DiscordUser? user = null)
        {
            user ??= ctx.User;

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
        [SlashCommand("info", "Get server info for a user")]
        public static async Task ServerInfo(InteractionContext ctx, [Option("user", "Specified user")] DiscordUser? user = null)
        {
            user ??= ctx.User;

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            var nickname = member.Nickname ?? member.DisplayName ?? user.Username;

            //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(user.AvatarUrl));
            var embed = new DiscordEmbedBuilder();


            embed.WithTitle($"{member.Nickname} ({user.Username}) server info");
            embed.AddField("Account Created:", $"{user.CreationTimestamp.ToString("yyyy/MM/dd  hh:mm")}", true);
            embed.AddField("Join date:", $"{member.JoinedAt.ToString("yyyy/MM/dd  hh:mm")}", true);
            //embed.AddField("Current Activity:", $"{user.Presence.Activity.ActivityType. ?? "Nothing"}", true);
            //embed.WithColor(DiscordColor.Orange);
            embed.WithColor(new DiscordColor(252, 177, 3));
            embed.WithThumbnail(user.AvatarUrl);
            await ctx.CreateResponseAsync(embed.Build());
            //Console.Write(user);
        }
    }
}