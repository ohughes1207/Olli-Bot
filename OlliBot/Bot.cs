using System.Text.Json;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using System.Text.Json.Serialization;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.Entities;
using OlliBot.Modules;
using OlliBot.Data;


namespace OlliBot
{
    public class Bot
    {
        internal Config Config {get; private set;}
        internal DiscordClient Client { get; private set; }
        internal SlashCommandsExtension Slash { get; private set; }

        public Bot()
        {
            Config = InitializeConfig();

            var clientConfig = new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All
                };

            Client = new DiscordClient(clientConfig);
            Slash = Client.UseSlashCommands();
        }
        
        public async Task RunAsync()
        {
            try
            {
                Console.Write(Config.OwnerID);

                Client.Ready += BotInitialization.InitializationTasks;
                
                
                /*var slashConfig = new SlashCommandsConfiguration
                {
                    
                };

                Slash = Client.UseSlashCommands(slashConfig);*/

                Slash.SlashCommandErrored += OnSlashError;
                SlashRegistry.RegisterCommands(Slash);

                await Client.ConnectAsync(new DiscordActivity("you", ActivityType.ListeningTo));
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running bot..\n------- EXCEPTION -------\n {ex.Message}\n------- EXCEPTION -------");
            }
        }
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
        /*
        private async Task OnSlashError(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            if (e.Exception is SlashExecutionChecksFailedException slashex)
            {

            }
            //Console.WriteLine(sender);
            Console.WriteLine(e.Exception);
        }
        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
        */
        private Config InitializeConfig()
        {
            var configFile = File.ReadAllText(@"config.json");
            Config = JsonSerializer.Deserialize<Config>(configFile) ?? throw new InvalidOperationException("Config cannot be null");
            return Config;
        }

        public async Task SaveConfig()
        {
            var config = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true});
            await File.WriteAllTextAsync($"config.json", config);
        }
    }
}