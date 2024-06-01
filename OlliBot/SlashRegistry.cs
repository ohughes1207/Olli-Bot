using DSharpPlus.SlashCommands;
using OlliBot.Modules;

namespace OlliBot
{
    internal static class SlashRegistry
    {
        //Register commands by adding the class that the commands belong to (all commands within a class will be added if multiple exist)
        internal static void RegisterCommands(SlashCommandsExtension slash)
        {
            slash.RegisterCommands<TestCommands>();
            slash.RegisterCommands<TestCommands2>();
            slash.RegisterCommands<BasicCommands>();
            //slash.RegisterCommands<TestToggleCommand>();
            slash.RegisterCommands<AdminCommands>();
            slash.RegisterCommands<DatabaseCommands>();
            slash.RegisterCommands<Emotes>();
            //slash.RegisterCommands<SetCommands>();
            //slash.RegisterCommands<HumbleBundleCommands>();
        }
    }
}