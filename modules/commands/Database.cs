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
        [SlashCommand("DBAddMessage", "Add a discord message to the database")]
        public async Task AddMessage(InteractionContext ctx,
        [Option("message", "Enter a message ID or quote content")] string messageEntry,
        [Option("title", "Title (Optional)")] string? Title = null,
        [Option("origin", "Quote origin (Optional)")] DiscordUser? User = null,
        [Choice("Meme", "Meme")] [Choice("Quote", "Quote")] [Choice("Other", "Other")] [Option("Type", "Type (If no value set then will be implicitly determined)")] string? MessageType = null)
        {
            if (!ulong.TryParse(messageEntry, out ulong result))
            {
                await AddQuoteManual(ctx, messageEntry, User);
                return;
            }
            await AddByID(ctx, messageEntry, Title, MessageType);
        }
        private async Task AddByID(InteractionContext ctx, string messageID, string? Title, string? MessageType)
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
        
        private async Task AddQuoteManual(InteractionContext ctx, string quoteContent, DiscordUser User)
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
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Entry added to the database").AsEphemeral());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

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
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(queriedMessage.Content));
                }
            }

        }
        private string GetMessageType(DiscordMessage message)
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