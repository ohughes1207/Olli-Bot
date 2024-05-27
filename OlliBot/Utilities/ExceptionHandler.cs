﻿using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus;

namespace OlliBot.Utilities
{
    public class ExceptionHandler
    {
        private readonly ILogger<Bot> _logger;

        public ExceptionHandler(ILogger<Bot> logger)
        {
            _logger = logger;
        }
        public async Task OnSlashError(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            // Check if the error is due to a failed check i.e on cooldown
            if (e.Exception is SlashExecutionChecksFailedException checksFailedException)
            {
                foreach (var check in checksFailedException.FailedChecks)
                {
                    // Slash command is on cooldown
                    if (check is SlashCooldownAttribute cooldown)
                    {
                        await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                        .WithContent($"This command is on cooldown\nCooldown Remaining: {Math.Round(cooldown.GetRemainingCooldown(e.Context).TotalSeconds, 1)} seconds.")
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
                _logger.LogError(e.Exception.Message);
            }
        }
    }
}
