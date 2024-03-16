using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace self_bot.modules.commands
{
    internal class DatabaseCommands : ApplicationCommandModule
    {
        [SlashCommand("AddDB", "Add an entry into the database")]
        public async Task AddMessage(InteractionContext ctx, [Option("user", "Specified user")] string MessageID)
        {
            var channel = ctx.Channel;
            
            ulong messageUlong = Convert.ToUInt64(MessageID);
            var message = await channel.GetMessageAsync(messageUlong);
            
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(message.Content));
        }
    }
}

