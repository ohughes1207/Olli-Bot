using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using OlliBot.Modules;
using Serilog;
using System;

namespace OlliBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);


            Serilog.ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);
            builder.Services.AddSingleton(logger);

            builder.Services.AddHostedService<Bot>();
            try
            {
                builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load config.json: {ex.Message}");
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
                        LoggerFactory = new LoggerFactory().AddSerilog(logger)
                    });

                    return discordClient;
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to initialize Discord client: {ex.Message}");
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
                logger.Error($"Host failed to run: {ex.Message}");
            }
        }
    }
}