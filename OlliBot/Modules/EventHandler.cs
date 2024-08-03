using Discord.WebSocket;
using Discord;
using System.Text;
using Discord.Interactions;

namespace OlliBot.Modules
{
    public class EventHandler
    {
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly ILogger<Bot> _logger;

        public EventHandler(IConfiguration configuration, DiscordSocketClient client, ILogger<Bot> logger)
        {
            _configuration = configuration;
            _client = client;
            _logger = logger;
        }

        public async Task OnMessage(SocketMessage message)
        {
            SocketGuildChannel channel = (SocketGuildChannel)message.Channel;
            var guild = channel.Guild;

            if (message.Content.Length > 150 && message.Author.Id != _client.CurrentUser.Id)
            {
                await message.Channel.SendMessageAsync("i ain't reading all that", messageReference: new MessageReference(message.Id));
                await message.Channel.SendMessageAsync("i'm happy for u tho");
                await message.Channel.SendMessageAsync("or sorry that happened");
                return;
            }

            if (message.Author.Id== 164740251934392321 && guild.Id.ToString() == _configuration["MainServer"] && new Random().Next(1, 101)<=15)
            {
                await message.Channel.SendMessageAsync("James Here", messageReference: new MessageReference(message.Id));
            }


            // Add more logic to this event
        }
        public Task OnSlashExecute(SlashCommandInfo slashInfo, IInteractionContext ctx, IResult result)
        {
            if (result.IsSuccess)
            {
                _logger.LogInformation("Command executed successfully");
            }
            else if (!result.IsSuccess)
            {
                if (result.Error == InteractionCommandError.UnmetPrecondition)
                {
                    _logger.LogWarning($"{result.ErrorReason}");
                }
            }
            return Task.CompletedTask;
        }
    }
}

/*
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
}*/