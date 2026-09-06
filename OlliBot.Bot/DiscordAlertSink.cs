using Discord;
using Serilog.Core;
using Serilog.Events;

namespace OlliBot.Bot;
internal class DiscordAlertSink(IDiscordClient discordClient, IConfiguration configuration) : ILogEventSink
{
    /// <summary>
    /// Send an alert via DMs to the owner of the bot with the log message when an error occurs.
    /// </summary>
    /// <param name="logEvent"></param>
    public async void Emit(LogEvent logEvent)
    {
        try
        {
            var userId = configuration.GetValue<ulong>("OwnerID", 119904333750861824);

            IUser? user = await discordClient.GetUserAsync(userId);

            if (user is null)
                return;

            string message = $"[{logEvent.Level}] {logEvent.RenderMessage()}";

            if (logEvent.Exception is not null)
            {
                message += $"\nException: {logEvent.Exception}";
            }

            await user.SendMessageAsync(message);
        }
        catch
        {

        }
    }
}
