using DSharpPlus;
using DSharpPlus.EventArgs;

namespace OlliBot.Modules
{
    public class BotInitialization
    {
        private readonly IConfiguration _configuration;
        private readonly Serilog.ILogger _logger;
        private readonly DiscordClient _discordClient;
        public BotInitialization(IConfiguration configuration, Serilog.ILogger logger, DiscordClient discordClient)
        {
            _configuration = configuration;
            _logger = logger;
            _discordClient = discordClient;
        }

        public async Task InitializationTasks(DiscordClient Client, ReadyEventArgs args)
        {
            _logger.Information("Initialization tasks running...");
            _configuration["BotID"] = _discordClient.CurrentUser.Id.ToString();
            _logger.Information($"Bot ID: {_configuration["BotID"]}");
            Client.MessageCreated += EventHandler.OnMessage;
            await Task.CompletedTask;
        }
    }
}