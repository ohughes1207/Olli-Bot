using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace OlliBot.Modules
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

    //Created a new class just to see how that interacts with registering new commannds
    public class Test_command2 : ApplicationCommandModule
    {

        [SlashCommand("test2", "Another slash command test")]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("wow").AsEphemeral());
        }
    }
}