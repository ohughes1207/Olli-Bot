using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using System.Text;

namespace OlliBot.Modules
{
    public class EventHandler
    {
        private readonly IConfiguration _configuration;

        public EventHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnMessage(DiscordClient Client, MessageCreateEventArgs args)
        {
            

            if (args.Message.Content.Length > 150 && args.Message.Author.Id!=Client.CurrentUser.Id)
            {
                await Client.SendMessageAsync(args.Channel, new DiscordMessageBuilder().WithReply(args.Message.Id).WithContent("i ain't reading all that"));
                await Client.SendMessageAsync(args.Channel, new DiscordMessageBuilder().WithContent("i'm happy for u tho"));
                await Client.SendMessageAsync(args.Channel, new DiscordMessageBuilder().WithContent("or sorry that happened"));
                return;
            }

            if (args.Message.Author.Id==164740251934392321 && args.Guild.Id.ToString() == _configuration["MainServer"] && new Random().Next(1, 101)<=15)
            {
                await Client.SendMessageAsync(args.Channel, new DiscordMessageBuilder().WithReply(args.Message.Id).WithContent("James here"));
            }


            // Add more logic to this event
        }

        public Task OnSlashInvoke(SlashCommandsExtension Slash, SlashCommandInvokedEventArgs args)
        {
            StringBuilder logMessage = new StringBuilder();
            logMessage.Append($"Command invoked: {args.Context.CommandName} ");

            if (args.Context.Interaction.Data.Options != null)
            {
                logMessage.Append("(");
                foreach (var option in args.Context.Interaction.Data.Options.Where(option => option != null))
                {
                    logMessage.Append($"{option.Name}:{option.Value}, ");
                }
                if (logMessage[logMessage.Length - 2] == ',') // Removing the trailing comma and space
                {
                    logMessage.Length -= 2;
                }
                logMessage.Append(") ");
            }
            logMessage.Append($"by {args.Context.User.Username}, {args.Context.User.Id}");

            if (!string.IsNullOrEmpty(args.Context.Member.Nickname))
            {
                logMessage.Append($" ({args.Context.Member.Nickname})");
            }

            Slash.Client.Logger.LogInformation($"{logMessage}");
            return Task.CompletedTask;
        }

        public Task OnSlashExecute(SlashCommandsExtension Slash, SlashCommandExecutedEventArgs args)
        {
            Slash.Client.Logger.LogInformation("Command executed successfully");
            return Task.CompletedTask;
        }
    }
}