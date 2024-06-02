using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Net.NetworkInformation;

namespace OlliBot.Modules
{
    public class BasicCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("avatar", "Get the avatar of the specified user")]
        public async Task Avatar([Summary("user", "Specified user")] SocketGuildUser? user = null)
        {
            user ??= Context.User as SocketGuildUser;

            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            var nickname = user.Nickname ?? user.DisplayName ?? user.Username;
            
            var embed = new EmbedBuilder();
            embed.WithTitle($"{nickname}'s avatar");
            embed.WithColor(new Color(252, 177, 3));
            embed.WithUrl(user.GetAvatarUrl(ImageFormat.Auto, 1024));
            embed.WithImageUrl(user.GetAvatarUrl(ImageFormat.Auto, 1024));

            await RespondAsync(embed: embed.Build());
        }
        
        [SlashCommand("info", "Get server info for a user")]
        public async Task ServerInfo([Summary("user", "Specified user")] SocketGuildUser? member = null)
        {
            member ??= Context.User as SocketGuildUser ?? Context.Guild.GetUser(Context.User.Id);

            var nickname = member.Nickname ?? member.DisplayName ?? member.Username;

            //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(user.AvatarUrl));
            var embed = new EmbedBuilder();


            embed.WithTitle($"{member.Nickname} ({member.Username}) server info");
            embed.AddField("Account Created:", $"{member.CreatedAt.ToString("yyyy/MM/dd  hh:mm")}", true);
            embed.AddField("Join date:", $"{member.JoinedAt.Value.ToString("yyyy/MM/dd  hh:mm")}", true);
            //embed.AddField("Current Activity:", $"{user.Presence.Activity.ActivityType. ?? "Nothing"}", true);
            //embed.WithColor(DiscordColor.Orange);
            embed.WithColor(new Color(252, 177, 3));
            embed.WithThumbnailUrl(member.GetAvatarUrl(ImageFormat.Auto, 1024));
            await RespondAsync(embed: embed.Build());
            //Console.Write(user);
        }
    }
}