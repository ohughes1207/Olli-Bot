using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using self_bot.modules.data;

namespace self_bot.modules.commands
{
    internal class DatabaseCommands : ApplicationCommandModule
    {
        [SlashCommand("AddDB", "Add an entry into the database")]
        public async Task AddMessage(InteractionContext ctx,
        [Option("message", "Message ID")] string MessageID,
        [Option("Title", "Title")] string? Title = null,
        [Choice("Meme", "Meme")] [Choice("Quote", "Quote")] [Choice("Other", "Other")] [Option("Type", "Type")] string? MessageType = null)
        {
            try
            {
                var channel = ctx.Channel;

                ulong messageUlong = Convert.ToUInt64(MessageID);
                var message = await channel.GetMessageAsync(messageUlong);

                if (MessageType == null)
                {
                    MessageType = GetMessageType(message, MessageType);
                }

                var attList = new List<string>();
                if (message.Attachments.Count > 0)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        attList.Add(attachment.Url);
                    }
                }

                using (var db = new MessageDB())
                {
                    var newMessage = new Message
                    {
                        DiscordMessageID = message.Id,
                        ServerID = ctx.Guild.Id,
                        Title = Title,
                        Content = message.Content,
                        AttachmentUrls = attList,
                        Author = ctx.User.Username,
                        AuthorID = ctx.User.Id,
                        MessageOriginID = message.Author.Id,
                        MessageType = MessageType,
                        DateTimeAdded = DateTime.UtcNow
                    };

                    await db.Messages.AddAsync(newMessage);
                    await db.SaveChangesAsync();
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Entry added to the database").AsEphemeral());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        private string GetMessageType(DiscordMessage message, string? messageType)
        {
            if (message.Attachments.Count > 0 )
            {
                return "Meme";
            }
            else
            {
                return "Quote";
            }
        }
    }
}