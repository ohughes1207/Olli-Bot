using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus;

namespace OlliBot.Utilities
{
    internal class ExceptionHandler
    {
        public static async Task OnSlashError(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            // Check if the error is due to a failed check i.e on cooldown
            if (e.Exception is SlashExecutionChecksFailedException checksFailedException)
            {
                foreach (var check in checksFailedException.FailedChecks)
                {
                    // Slash command is on cooldown
                    if (check is SlashCooldownAttribute cooldown)
                    {
                        var cd = Math.Round(cooldown.GetRemainingCooldown(e.Context).TotalSeconds, 1);
                        sender.Client.Logger.LogError($"Command is on cooldown (Cooldown remaining: {cd})");
                        await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                        .WithContent($"This command is on cooldown\nCooldown Remaining: {cd} seconds.")
                        .AsEphemeral(true));
                    }
                    // Command failed because some other check failed (issue with permissions maybe?)
                    else if (check is SlashRequireUserPermissionsAttribute perms)
                    {
                        sender.Client.Logger.LogWarning($"{e.Context.User} attmpted to invoke {e.Context.CommandName} without permissions");
                        await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                        .WithContent("You do not have permission to use this command.")
                        .AsEphemeral(true));
                    }
                    else
                    {
                        sender.Client.Logger.LogWarning($"{e.Context.User} attmpted to invoke {e.Context.CommandName} and something went wrong");
                        await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                        .WithContent($"Couldn't invoke command for some reason lmao report it to {e.Context.Guild.GetMemberAsync(119904333750861824)}")
                        .AsEphemeral(true));
                    }
                }
            }
            else
            {
                // Case for typical exceptions
                sender.Client.Logger.LogError(e.Exception.Message);
            }
        }
    }
}
