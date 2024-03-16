using DSharpPlus.SlashCommands;
using self_bot.modules.admin;
using self_bot.modules.commands;

namespace self_bot
{
    internal static class SlashRegistry
    {
        //Register commands by adding the class that the commands belong to (all commands within a class will be added if multiple exist)
        internal static void RegisterCommands(SlashCommandsExtension slash)
        {
            slash.RegisterCommands<Test_command>();
            slash.RegisterCommands<Test_command2>();
            slash.RegisterCommands<SimpleCommands>();
            //slash.RegisterCommands<MeowCommand>();
            slash.RegisterCommands<AdminCommands>();
            slash.RegisterCommands<DatabaseCommands>();
            //slash.RegisterCommands<SetCommands>();
            //slash.RegisterCommands<HumbleBundleCommands>();
        }
    }
}