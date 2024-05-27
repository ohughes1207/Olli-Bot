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
                    else
                    {
                        await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                        .WithContent("You do not have permission to use this command.")
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
