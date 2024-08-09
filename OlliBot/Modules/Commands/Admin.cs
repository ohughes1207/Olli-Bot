using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace OlliBot.Modules
{
    public class AdminCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("purge", "Purge a number of messages from a user in a text channel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Purge([Summary("user", "Specified user")] SocketGuildUser user, [Summary("amount", "amount of messages to delete")] int amount)
        {
            try
            {
                //Temp safety precaution to avoid someone mass deleting messages on accident
                if (amount > 20)
                {
                    await Context.Interaction.RespondAsync("No more than 20 messages pls (this is temp lol)", ephemeral: true);
                    return;
                }


                var messageList = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
                var filteredMessages = from m in messageList
                                       where m.Author.Id == user.Id
                                       select m;

                var delMessages = filteredMessages.Take(amount);

                //Console.WriteLine((DateTimeOffset.UtcNow - delMessages.First().Timestamp).TotalDays);

                //Messages older than 14 days can't be bulk deleted so we split old and recent messages into two lists and delete them using appropriate methods

                //Messages older than 13.5 days
                var oldMessages = from m in delMessages where (DateTimeOffset.UtcNow - m.Timestamp).TotalDays > 13.5 select m;

                //Every other message in delMessages not in oldMessages
                var recentMessages = delMessages.Except(oldMessages);

                await Context.Interaction.DeferAsync(ephemeral: true);

                var channel = (ITextChannel)Context.Channel;

                if (recentMessages.Any())
                {
                    await channel.DeleteMessagesAsync(recentMessages);
                }

                foreach (IMessage m in oldMessages)
                {
                    await Context.Channel.DeleteMessageAsync(m);
                }
                await Context.Interaction.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = $"lmao I just deleted {recentMessages.Count()} messages by {user.Username}";
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Context.Interaction.RespondAsync($"Error happened: {ex.Message}");
            }
        }
    }
}