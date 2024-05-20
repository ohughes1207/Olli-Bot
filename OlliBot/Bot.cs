using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using OlliBot.Modules;

namespace OlliBot;

public class Bot : BackgroundService
{
    private readonly ILogger<Bot> _logger;

    private readonly DiscordClient _discordClient;

    private readonly SlashCommandsExtension _slash;
    public Bot(ILogger<Bot> logger, DiscordClient discordClient, SlashCommandsExtension slash)
    {
        _logger = logger;
        _discordClient = discordClient;
        _slash = slash;
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Started");
        //_discordClient.Ready += BotInitialization.InitializationTasks;
        //_slash.SlashCommandErrored += OnSlashError;
        await _discordClient.ConnectAsync(new DiscordActivity("you :3", ActivityType.ListeningTo));
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("STOPPING");
        await _discordClient.DisconnectAsync();
        _discordClient.Dispose();
        _logger.LogInformation("Bot client disposed.");
        //_slash.Dispose();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //Console.WriteLine("Executed");
        return Task.CompletedTask;
    }

    //Move this logic to a new class
    private async Task OnSlashError(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
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
                    .WithContent($"This command is on cooldown. Please wait {Math.Round(cooldown.GetRemainingCooldown(e.Context).TotalSeconds, 1)} seconds.")
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
            Console.WriteLine(e.Exception);
        }
    }
}
