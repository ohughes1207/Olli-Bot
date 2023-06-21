using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace self_bot.modules.commands
{
    public class Test_command : ApplicationCommandModule
    {
        [SlashCommand("test1", "A slash command test")]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("All systems functional\nTest success"));
        }

        [SlashCommand("test3", "3rd slash command test")]
        public async Task TestCommand2(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Yeah...\nthis works"));
        }
    }
    public class Test_command2 : ApplicationCommandModule
    {

        [SlashCommand("test2", "Another slash command test")]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("BEEP BOOP!\nTest success"));
        }
    }
}