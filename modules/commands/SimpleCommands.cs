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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(user.AvatarUrl));
        }
    }
}