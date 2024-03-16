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
        public async Task AddMessage(InteractionContext ctx, [Option("message", "Message ID")] string MessageID)
        {
            var channel = ctx.Channel;
            
            ulong messageUlong = Convert.ToUInt64(MessageID);
            var message = await channel.GetMessageAsync(messageUlong);

            //Console.WriteLine(message.Attachments.);
            
            //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(message.Content));

            var responseBuilder = new DiscordInteractionResponseBuilder();

            string responseContent = message.Content;

            // Check if the message has attachments
            if (message.Attachments.Count > 0)
            {
                // For each attachment, add a new embed to the response
                foreach (var attachment in message.Attachments)
                {
                    responseContent += $"{Environment.NewLine} {attachment.Url}";
                }
            }

            responseBuilder.WithContent(responseContent);

            // Send the response with the original message content and any attachments as embeds
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);

        }
    }
}

