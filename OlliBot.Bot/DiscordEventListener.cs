using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using OlliBot.Bot.Notifications;

namespace OlliBot.Bot;

public class DiscordEventListener(IServiceScopeFactory serviceScope)
{
    private readonly CancellationToken _cancellationToken = new CancellationTokenSource().Token;

    private IPublisher Publisher
    {
        get
        {
            IServiceScope scope = serviceScope.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IPublisher>();
        }
    }

    internal Task OnMessageReceivedAsync(SocketMessage message)
    {
        return Publisher.Publish(new MessageReceivedNotification(message), _cancellationToken);
    }

    internal Task OnInteractionCreated(SocketInteraction interaction)
    {
        return Publisher.Publish(new InteractionCreatedNotification(interaction), _cancellationToken);
    }

    internal Task OnClientReady()
    {
        return Publisher.Publish(new ClientReadyNotification(), _cancellationToken);
    }

    internal Task OnCommandExecuted(ICommandInfo info, IInteractionContext context, IResult result)
    {
        return Publisher.Publish(new CommandExecutedNotification(info, context, result), _cancellationToken);
    }
}
