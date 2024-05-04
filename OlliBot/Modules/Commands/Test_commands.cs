using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace self_bot.modules.commands
{
    public class TestCommands : ApplicationCommandModule
    {
        [SlashCommand("test1", "A slash command test")]
        [SlashCooldown(1, 10, SlashCooldownBucketType.User)]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("All systems functional\nTest success").AsEphemeral());
        }

        [SlashCommand("test3", "3rd slash command test")]
        public async Task TestCommand2(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("womp womp").AsEphemeral());
        }
    }
    public class Test_command2 : ApplicationCommandModule
    {

        [SlashCommand("test2", "Another slash command test")]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("wow").AsEphemeral());
        }
    }
}