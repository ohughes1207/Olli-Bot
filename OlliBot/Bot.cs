using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using OlliBot.Modules;
using OlliBot.Utilities;

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
        _logger.LogInformation("OlliBot starting...");
        _discordClient.Ready += BotInitialization.InitializationTasks;
        _slash.SlashCommandErrored += ExceptionHandler.OnSlashError;

        await _discordClient.ConnectAsync(new DiscordActivity("you :3", ActivityType.Watching));
    }


    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _discordClient.Ready -= BotInitialization.InitializationTasks;
            _slash.SlashCommandErrored -= ExceptionHandler.OnSlashError;


            _logger.LogInformation("OlliBot disconnecting...");
            await _discordClient.DisconnectAsync();
            _logger.LogInformation("Ollibot disconnected...");


            _logger.LogInformation("Disposing client...");
            _slash.Dispose();
            _discordClient.Dispose();
            _logger.LogInformation("Bot client disposed...");


            _logger.LogInformation("OlliBot shutting down...");
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex.Message);
        }
    }


    //Only returns Task.CompletedTask to satisfy implementation requirement for BackgroundService
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.CompletedTask;
}
