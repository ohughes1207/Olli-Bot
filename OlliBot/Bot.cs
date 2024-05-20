using DSharpPlus;
using DSharpPlus.Entities;
using OlliBot.Modules;

namespace OlliBot;

public class Bot : BackgroundService
{
    private readonly ILogger<Bot> _logger;

    private readonly DiscordClient _discordClient;

    public Bot(ILogger<Bot> logger, DiscordClient discordClient)
    {
        _logger = logger;
        _discordClient = discordClient;

    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Started");
        _discordClient.Ready += BotInitialization.InitializationTasks;
        await _discordClient.ConnectAsync(new DiscordActivity("you", ActivityType.ListeningTo));
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        //_discordClient.Dispose();
        await _discordClient.DisconnectAsync();
        //_discordClient.Dispose();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Executed");
        return Task.CompletedTask;
    }
}
