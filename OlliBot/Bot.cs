using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using OlliBot.Modules;
using OlliBot.Utilities;
using Serilog;

namespace OlliBot;

public class Bot : BackgroundService
{

    private readonly Serilog.ILogger _logger;
    private readonly DiscordClient _discordClient;
    private readonly SlashCommandsExtension _slash;
    private readonly IConfiguration _configuration;
    private readonly BotInitialization _botInitialization;

    public Bot(Serilog.ILogger logger, DiscordClient discordClient, SlashCommandsExtension slash, IConfiguration configuration, BotInitialization botInitialization)
    {
        //_logger = Log.ForContext<Bot>();
        _logger = logger;
        _discordClient = discordClient;
        _slash = slash;
        _configuration = configuration;
        _botInitialization = botInitialization;
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("OlliBot starting...");
        _discordClient.Ready += _botInitialization.InitializationTasks;
        _slash.SlashCommandErrored += ExceptionHandler.OnSlashError;

        _logger.Information(_configuration["OwnerID"] ?? "Owner ID not configured");

        await _discordClient.ConnectAsync(new DiscordActivity("you :3", ActivityType.Playing));
    }


    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _discordClient.Ready -= _botInitialization.InitializationTasks;
            _slash.SlashCommandErrored -= ExceptionHandler.OnSlashError;


            _logger.Information("OlliBot disconnecting...");
            await _discordClient.DisconnectAsync();
            _logger.Information("Ollibot disconnected...");


            _logger.Information("Disposing client...");
            _slash.Dispose();
            _discordClient.Dispose();
            _logger.Information("Bot client disposed...");

            _logger.Information("Flushing logs...");
            Log.CloseAndFlush();


            _logger.Information("OlliBot shutting down...");

        }
        catch (Exception ex) 
        {
            _logger.Error(ex.Message);
        }
    }


    //Only returns Task.CompletedTask to satisfy implementation requirement for BackgroundService
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.CompletedTask;
}
