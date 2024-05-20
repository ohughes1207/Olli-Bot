using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace OlliBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<Bot>();

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

                var slash = discordClient.UseSlashCommands(new SlashCommandsConfiguration
                {
                    Services = serviceProvider
                });

                SlashRegistry.RegisterCommands(slash);

                return discordClient;
            });

            var host = builder.Build();
            //Console.Write(builder.Services.GetType());
            host.Run();
        }
    }
}