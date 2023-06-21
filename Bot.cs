using System.Text.Json;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using JsonConfig;

namespace self_bot
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
        public SlashCommandsExtension Slash { get; private set; }
        
        public async Task RunAsync()
        {
            try
            {
            var config = InitializeConfig();
            
            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;
            
            
            var slashConfig = new SlashCommandsConfiguration
            {
                
            };

            Slash = Client.UseSlashCommands(slashConfig);

            SlashRegistry.RegisterCommands(Slash);

            await Client.ConnectAsync();
            await Task.Delay(-1);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running bot..\n------- EXCEPTION -------\n {ex.Message}\n------- EXCEPTION -------");
            }
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
        
        private static DiscordConfiguration InitializeConfig()
        {
            var jsonString = File.ReadAllText("config.json");

            var configJson = JsonSerializer.Deserialize<ConfigJson>(jsonString);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            return config;
        }
    }
}
