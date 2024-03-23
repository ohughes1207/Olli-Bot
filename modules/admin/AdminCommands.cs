using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;
using DSharpPlus;
using DSharpPlus.Entities;
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
                int amount = (int)x;
                var messageList = await ctx.Channel.GetMessagesAsync(500);
                var filteredMessages =  from m in messageList
                                        where m.Author == user
                                        select m;

                var x_Messages_del = filteredMessages.Take(amount);
                await ctx.Channel.DeleteMessagesAsync(x_Messages_del);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"---PURGE COMPLETE---\n{x_Messages_del.Count()} MESSAGES DELETED\n---PURGE COMPLETE---").AsEphemeral());
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"---EXCEPTION---\n{ex.Message}\n---EXCEPTION---"));
            }
        }
    }
}