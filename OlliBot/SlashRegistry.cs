using Discord.Interactions;

namespace OlliBot
{
    internal static class SlashRegistry
    {
        //Register commands by adding the class that the commands belong to (all commands within a class will be added if multiple exist)
        internal static void RegisterCommands(InteractionService interaction)
        {
            interaction.AddCommandsGloballyAsync();
            //interaction.AddCommandsGloballyAsync<TestCommands2>();
            //interaction.AddCommandsGloballyAsync<BasicCommands>();

            //slash.RegisterCommands<TestToggleCommand>();

            //interaction.AddCommandsGloballyAsync<AdminCommands>();
            //interaction.AddCommandsGloballyAsync<DatabaseCommands>();
            //interaction.AddCommandsGloballyAsync<Emotes>();

            //slash.RegisterCommands<SetCommands>();
            //slash.RegisterCommands<HumbleBundleCommands>();
        }
    }
}