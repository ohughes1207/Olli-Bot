using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace OlliBot.Modules
{
    internal class ConfigCommands : ApplicationCommandModule
    {
        [SlashCommand("cfg", "Edit the config")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task Config(InteractionContext ctx, [Choice("user", "Specified user")] string setting)
        {

        }
    }
}