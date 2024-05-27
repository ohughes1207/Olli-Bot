using DSharpPlus;
using DSharpPlus.SlashCommands;
using OlliBot.Modules;
using OlliBot.Utilities;
using Serilog;

namespace OlliBot
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();
            

            builder.Services.AddHostedService<Bot>();

            try
            {
                builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load config.json: {ex.Message}");
                return;
            }

            builder.Services.AddSingleton<DiscordClient>((serviceProvider) =>
            {
                var config = builder.Configuration;

                try
                {
                    var discordClient = new DiscordClient(new DiscordConfiguration
                    {
                        Token = config["DiscordBotToken"],
                        TokenType = TokenType.Bot,
                        AutoReconnect = true,
                        Intents = DiscordIntents.All,
                        LoggerFactory = new LoggerFactory().AddSerilog(Log.Logger)
                    });

                    return discordClient;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to initialize Discord client: {ex.Message}");
                    throw;
                }
            });

            builder.Services.AddSingleton<SlashCommandsExtension>((serviceProvider) =>
            {
                var discordClient = serviceProvider.GetRequiredService<DiscordClient>();

                var slash = discordClient.UseSlashCommands(new SlashCommandsConfiguration
                {
                    Services = serviceProvider
                });

                SlashRegistry.RegisterCommands(slash);

                return slash;
            });


            builder.Services.AddTransient<BotInitialization>();

            try
            {
                var host = builder.Build();
                host.Run();
            }
            catch (Exception ex) 
            {
                Log.Error($"Host failed to run: {ex.Message}");
            }
            finally
            {
                Log.Information("Flushing logs...");
                await Log.CloseAndFlushAsync();
            }
        }
    }
}