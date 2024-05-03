using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace self_bot.modules.admin
{
    internal class AdminCommands : ApplicationCommandModule
    {
        [SlashCommand("purge", "Purge the last x amount of messages from a user")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task Purge(InteractionContext ctx, [Option("user", "Specified user")] DiscordUser user, [Option("amount", "Amount of messages to purge")] long x)
        {
            try
            {

                //Temp safety procaution to avoid someone mass deleting messages on accident
                if (x > 20)
                {
                    return;
                }


                var messageList = await ctx.Channel.GetMessagesAsync(100);
                var filteredMessages =  from m in messageList
                                        where m.Author == user
                                        select m;
                int amount = (int)x;
                var delMessages = filteredMessages.Take(amount);

                Console.WriteLine((DateTimeOffset.UtcNow - delMessages.First().Timestamp).TotalDays);

                //Messages older than 14 days can't be bulk deleted so we split old and recent messages into two lists and delete them using appropriate methods

                //Messages older than 13.5 days
                var oldMessages = from m in delMessages where (DateTimeOffset.UtcNow - m.Timestamp).TotalDays > 13.5 select m;

                //Every other message in delMessages not in oldMessages
                var recentMessages = delMessages.Except(oldMessages);

                await ctx.DeferAsync(true);

                //Bulk delete recent messages
                if (recentMessages.Count() > 0 )
                {
                    await ctx.Channel.DeleteMessagesAsync(recentMessages);
                }

                //Loop through all old messages to delete
                foreach (DiscordMessage m in oldMessages)
                {
                    await ctx.Channel.DeleteMessageAsync(m);
                }

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"---PURGE COMPLETE---\n{recentMessages.Count()} MESSAGES DELETED\n---PURGE COMPLETE---"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"---EXCEPTION---\n{ex.Message}\n---EXCEPTION---"));
            }
        }
    }
}