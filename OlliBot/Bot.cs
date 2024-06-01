using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using OlliBot.Modules;
using OlliBot.Utilities;

namespace OlliBot;

public class Bot : BackgroundService
{

    private readonly ILogger<Bot> _logger;
    private readonly DiscordClient _discordClient;
    private readonly SlashCommandsExtension _slash;
    private readonly IConfiguration _configuration;
    private readonly BotInitialization _botInitialization;
    private readonly OlliBot.Modules.EventHandler _eventHandler;

    public Bot(ILogger<Bot> logger, DiscordClient discordClient, SlashCommandsExtension slash, IConfiguration configuration, BotInitialization botInitialization, OlliBot.Modules.EventHandler eventHandler)
    {
        //_logger = Log.ForContext<Bot>();
        _logger = logger;
        _discordClient = discordClient;
        _slash = slash;
        _configuration = configuration;
        _botInitialization = botInitialization;
        _eventHandler = eventHandler;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OlliBot starting...");

        _discordClient.Ready += _botInitialization.InitializationTasks;

        _discordClient.MessageCreated += _eventHandler.OnMessage;
        _slash.SlashCommandErrored += ExceptionHandler.OnSlashError;
        _slash.SlashCommandInvoked += _eventHandler.OnSlashInvoke;
        _slash.SlashCommandExecuted += _eventHandler.OnSlashExecute;

        _logger.LogInformation(_configuration["OwnerID"] ?? "Owner ID not configured");
        try
        {
            await _discordClient.ConnectAsync(new DiscordActivity("with you :3", ActivityType.Playing));
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Client failed to connect: {ex.Message}");

            _discordClient.Ready -= _botInitialization.InitializationTasks;

            _discordClient.MessageCreated -= _eventHandler.OnMessage;
            _slash.SlashCommandErrored -= ExceptionHandler.OnSlashError;
            _slash.SlashCommandInvoked -= _eventHandler.OnSlashInvoke;
            _slash.SlashCommandExecuted -= _eventHandler.OnSlashExecute;

            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _discordClient.Ready -= _botInitialization.InitializationTasks;
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
            _logger.LogCritical($"Error occured while shutting down: {ex.Message}");
            throw;
        }
    }


    //Only returns Task.CompletedTask to satisfy implementation requirement for BackgroundService
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.CompletedTask;
}
