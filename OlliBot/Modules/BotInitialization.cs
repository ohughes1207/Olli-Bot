using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace OlliBot.Modules
{
    public class BotInitialization
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Bot> _logger;
        public BotInitialization(IConfiguration configuration, ILogger<Bot> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InitializationTasks(DiscordClient Client, ReadyEventArgs args)
        {
            _logger.LogInformation("Initialization tasks running...");
            _configuration["BotID"] = Client.CurrentUser.Id.ToString();
            _logger.LogInformation($"Bot ID: {_configuration["BotID"]}");

            Client.MessageCreated += EventHandler.OnMessage;

            await Task.CompletedTask;
        }
    }
}