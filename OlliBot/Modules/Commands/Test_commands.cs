using Discord.Interactions;

namespace OlliBot.Modules
{
    public class TestCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("test1", "A slash command test")]
        public async Task TestCommand()
        {
            await Context.Interaction.RespondAsync("All systems functional\nTest success", ephemeral: true);
        }

        [SlashCommand("test3", "3rd slash command test")]
        public async Task TestCommand2()
        {
            await Context.Interaction.RespondAsync("womp womp", ephemeral: true);
        }
    }
    public class TestCommands2 : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("test2", "Another slash command test")]
        public async Task TestCommand()
        {
            await Context.Interaction.RespondAsync("wow", ephemeral: true);
        }
    }
}