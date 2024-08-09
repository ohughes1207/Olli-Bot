using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OlliBot.Modules;

namespace OlliBot;

public class Bot : BackgroundService
{

    private readonly ILogger<Bot> _logger;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interaction;
    private readonly IConfiguration _configuration;
    private readonly BotInitialization _botInitialization;
    private readonly InteractionHandler _interactionHandler;
    private readonly OlliBot.Modules.EventHandler _eventHandler;

    public Bot(ILogger<Bot> logger, DiscordSocketClient client, InteractionService interaction, IConfiguration configuration , BotInitialization botInitialization, InteractionHandler interactionHandler, OlliBot.Modules.EventHandler eventHandler)
    {
        //_logger = Log.ForContext<Bot>();
        _logger = logger;
        _client = client;
        _interaction = interaction;
        _configuration = configuration;
        _botInitialization = botInitialization;
        _interactionHandler = interactionHandler;
        _eventHandler = eventHandler;

    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OlliBot starting...");
        _client.Ready += _botInitialization.InitializationTasks;

        _client.InteractionCreated += _interactionHandler.HandleInteraction;
        _client.InteractionCreated += _interactionHandler.OnSlashInvoked;

        _client.MessageReceived += _eventHandler.OnMessage;

        _interaction.SlashCommandExecuted += _eventHandler.OnSlashExecute;

        _logger.LogInformation(_configuration["OwnerID"] ?? "Owner ID not configured");

        try
        {
            await _client.LoginAsync(TokenType.Bot, _configuration["DiscordBotToken"]);
            await _client.StartAsync();
        }
        catch (Exception ex)
        {

            _logger.LogCritical($"Client failed to connect: {ex.Message}");

            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {

            _client.Ready -= _botInitialization.InitializationTasks;

            _client.InteractionCreated -= _interactionHandler.HandleInteraction;
            _client.InteractionCreated -= _interactionHandler.OnSlashInvoked;

            _client.MessageReceived -= _eventHandler.OnMessage;

            _interaction.SlashCommandExecuted -= _eventHandler.OnSlashExecute;

            _logger.LogInformation("OlliBot disconnecting...");
            await _client.StopAsync();
            await _client.LogoutAsync();
            _logger.LogInformation("Ollibot disconnected...");

            _logger.LogInformation("Disposing client...");
            _interaction.Dispose();
            _client.Dispose();
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
