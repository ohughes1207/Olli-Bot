using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using self_bot.modules.data;

namespace self_bot.modules.commands
{
    internal class DatabaseCommands : ApplicationCommandModule
    {
        //Command to add entries to database
        [SlashCommand("DBAdd", "Add a discord message to the database")]
        public async Task AddMessage(InteractionContext ctx,
        [Option("message", "Enter a message ID or quote content")] string messageEntry,
        [Option("title", "Title (Optional)")] string? Title = null,
        [Option("origin", "Quote origin (Optional if using Message ID for input)")] DiscordUser? User = null,
        [Choice("Meme", "Meme")] [Choice("Quote", "Quote")] [Choice("Other", "Other")] [Option("Type", "Type (If no value set then will be implicitly determined)")] string? MessageType = null)
        {
            //If input for message is not convertable to ulong assume it is a manually entered quote
            if (!ulong.TryParse(messageEntry, out ulong result))
            {
                //If manually entered quote has no origin then respond with unsuccessful message
                if (User == null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Entry unsuccessful, try again with a quote origin.").AsEphemeral());
                    return;
                }
                //Add the quote to the database
                await DatabaseLogic.AddQuoteManual(ctx, messageEntry, User);
                return;
            }
            //If input for message is ulong then add to database from the messageID
            await DatabaseLogic.AddByID(ctx, messageEntry, Title, MessageType);
        }
        //Command to call an entry from the database based on ID
        [SlashCommand("DBCall", "Call entry by ID from the database")]
        public async Task CallMessage(InteractionContext ctx,
        [Option("Id", "Message ID")] double DbID)
        {
            using (var db = new MessageDB())
            {
                var queriedMessage = db.Messages.AsQueryable().Where(x => x.ID == DbID && x.ServerID == ctx.Guild.Id).FirstOrDefault();

                if (queriedMessage.DiscordMessageID == 0 && queriedMessage.MessageType=="Quote")
                {
                    DiscordMember quoteOrigin = await ctx.Guild.GetMemberAsync(queriedMessage.MessageOriginID);
                    string responseContent = $"\"{queriedMessage.Content}\" - {quoteOrigin.DisplayName}";
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(responseContent));
                }
                else
                {
                    var responseBuilder = new DiscordInteractionResponseBuilder();
                    string responseContent = queriedMessage.Content;
                    
                    if (queriedMessage.AttachmentUrls.Count > 0)
                    {
                        responseContent+=Environment.NewLine;
                        foreach (var attachment in queriedMessage.AttachmentUrls)
                        {
                            responseContent+=attachment;
                        }
                    }
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(responseContent));
                }
            }

        }
        [SlashCommand("DBDelete", "Delete an entry from the database")]
        public async Task DeleteEntry(InteractionContext ctx,
        [Option("Id", "Message ID")] double DbID)
        {
            using (var db = new MessageDB())
            {
                var queriedMessage = db.Messages.AsQueryable().Where(x => x.ID == DbID && x.ServerID == ctx.Guild.Id).FirstOrDefault();

                if (queriedMessage.AuthorID!=ctx.User.Id && !ctx.Member.Permissions.HasPermission(Permissions.Administrator))
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(ctx.Member.Permissions.HasPermission(Permissions.Administrator).ToString()));
                    return;
                }
                db.Messages.Remove(queriedMessage);
                db.SaveChanges();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Deleted entry").AsEphemeral());
            }
        }
    }

    internal class DatabaseLogic
    {
        internal static string GetMessageType(DiscordMessage message)
        {
            if (message.Attachments.Count > 0  || message.Embeds.Count > 0)
            {
                return "Meme";
            }
            else if (message.Content != null)
            {
                return "Quote";
            }
            else
            {
                return "Other";
            }
        }
        internal static async Task AddByID(InteractionContext ctx, string messageID, string? Title, string? MessageType)
        {
            try
            {
                var channel = ctx.Channel;

                ulong messageUlong = Convert.ToUInt64(messageID);
                var message = await channel.GetMessageAsync(messageUlong);

                if (MessageType == null)
                {
                    MessageType = GetMessageType(message);
                }
                string messageContent = message.Content;
                var attList = new List<string>();

                //Attachment is a file upload attached to a message
                if (message.Attachments.Count > 0)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        attList.Add(attachment.Url);
                    }
                }
                //Embed is a image/video embedded from a link
                if (message.Embeds.Count > 0)
                {
                    foreach (var embed in message.Embeds)
                    {
                        attList.Add(embed.Url.AbsoluteUri);
                        messageContent = messageContent.Replace(embed.Url.AbsoluteUri, "");
                    }
                    messageContent=messageContent.Trim();
                }

                using (var db = new MessageDB())
                {
                    var newMessage = new Message
                    {
                        DiscordMessageID = message.Id,
                        ServerID = ctx.Guild.Id,
                        Title = Title,
                        Content = messageContent,
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
        
        internal static async Task AddQuoteManual(InteractionContext ctx, string quoteContent, DiscordUser User)
        {
            try
            {
                using (var db = new MessageDB())
                {
                    var newQuote = new Message
                    {
                        ServerID = ctx.Guild.Id,
                        Content = quoteContent,
                        Author = ctx.User.Username,
                        AuthorID = ctx.User.Id,
                        MessageOriginID = User.Id,
                        MessageType = "Quote",
                        DateTimeAdded = DateTime.UtcNow
                    };

                    await db.Messages.AddAsync(newQuote);
                    await db.SaveChangesAsync();
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Entry added to the database"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}