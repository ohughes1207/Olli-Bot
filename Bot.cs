using System.Text.Json;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using System.Text.Json.Serialization;

namespace self_bot
{
    public class Bot
    {
        public static Config Config {get; private set;}
        public static DiscordClient Client { get; private set; }
        public SlashCommandsExtension Slash { get; private set; }
        
        public async Task RunAsync()
        {
            try
            {
                await InitializeConfig();

                Console.Write(Config.OwnerID);

                var clientConfig = new DiscordConfiguration
                {
                    Token = Config.Token,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true
                };
                
                Client = new DiscordClient(clientConfig);

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
        
        private Task InitializeConfig()
        {
            var configFile = File.ReadAllText(@"config.json");

            Config = JsonSerializer.Deserialize<Config>(configFile);

            return Task.CompletedTask;
        }
    }
    public readonly struct Config
    {
        //Rename JsonPropertyName to JsonProperty if Newtonsoft.json is prefered
        [JsonPropertyName("token")]
        public string Token { get; init; }
        [JsonPropertyName("BotChannel")]
        public ulong BotChannel { get; init; }
        [JsonPropertyName("OwnerID")]
        public ulong OwnerID { get; init; }
        [JsonPropertyName("BotID")]
        internal ulong BotID { get; init; }
    }
}