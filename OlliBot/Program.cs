using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using OlliBot.Modules;
using Serilog;

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

            builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);

            builder.Services.AddSingleton<DiscordClient>((serviceProvider) =>
            {
                var config = builder.Configuration;

                var discordClient = new DiscordClient(new DiscordConfiguration
                {
                    Token = config["DiscordBotToken"],
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    Intents = DiscordIntents.All,
                    LoggerFactory= new LoggerFactory().AddSerilog(logger)
                });

                /*
                var slash = discordClient.UseSlashCommands(new SlashCommandsConfiguration
                {
                    Services = serviceProvider
                });

                SlashRegistry.RegisterCommands(slash);
                */

                return discordClient;
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


            var host = builder.Build();
            host.Run();

            /*
            try
            {
                host.Run();
            }
            finally
            {
                logger.Information("Flushing logs...");
                Log.CloseAndFlush();
            }*/

        }
    }
}