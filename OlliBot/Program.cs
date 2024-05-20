using DSharpPlus;
using DSharpPlus.SlashCommands;
using OlliBot.Data;

namespace OlliBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

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
                    Intents = DiscordIntents.All
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

            Console.WriteLine(builder.Configuration["BotID"]);

            var host = builder.Build();
            //Console.Write(builder.Services.GetType());
            host.Run();
        }
    }
}