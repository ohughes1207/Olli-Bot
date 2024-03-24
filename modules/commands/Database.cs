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
                //Call method to add manually entered quotes
                await DatabaseLogic.AddQuoteManual(ctx, messageEntry, User);
                return;
            }
            //If input for message is ulong then call method to add method to database from it's Id
            await DatabaseLogic.AddByID(ctx, messageEntry, Title, MessageType);
        }
        //Command to call an entry from the database based on ID
        [SlashCommand("DBCall", "Call entry by ID from the database")]
        public async Task CallMessage(InteractionContext ctx,
        [Option("Query", "Message ID or Title")] string query)
        {
            using (var db = new MessageDB())
            {
                var guildMessages = db.Messages.AsQueryable().Where(x=> x.GuildId == ctx.Guild.Id);
                
                var queriedMessage = default(Message);

                if (int.TryParse(query, out int intQuery))
                {
                    queriedMessage = guildMessages.Where(x => x.Id == intQuery).FirstOrDefault();
                }
                else
                {
                    queriedMessage = guildMessages.Where(x => x.Title.ToLower().Contains(query.ToLower())).FirstOrDefault();

                }
                var responseBuilder = new DiscordInteractionResponseBuilder();

                if (queriedMessage == null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder.WithContent("No message found").AsEphemeral());
                    return;
                }

                if (queriedMessage.DiscordMessageId == null && queriedMessage.MessageType=="Quote")
                {
                    DiscordMember quoteOrigin = await ctx.Guild.GetMemberAsync(queriedMessage.MessageOriginId);
                    string responseContent = $"\"{queriedMessage.Content}\" - {quoteOrigin.DisplayName}";
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder.WithContent(responseContent));
                }
                else
                {

                    string responseContent = queriedMessage.Content;
                    
                    if (queriedMessage.AttachmentUrls.Count > 0)
                    {
                        responseContent+=Environment.NewLine;
                        foreach (var attachment in queriedMessage.AttachmentUrls)
                        {
                            responseContent+=attachment;
                        }
                    }
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder.WithContent(responseContent));
                }
            }

        }
        [SlashCommand("DBDelete", "Delete an entry from the database")]
        public async Task DeleteEntry(InteractionContext ctx,
        [Option("Id", "Message ID")] double DbID)
        {
            using (var db = new MessageDB())
            {
                var queriedMessage = db.Messages.AsQueryable().Where(x => x.Id == DbID && x.GuildId == ctx.Guild.Id).FirstOrDefault();

                if (queriedMessage.AuthorId!=ctx.User.Id && !ctx.Member.Permissions.HasPermission(Permissions.Administrator))
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(ctx.Member.Permissions.HasPermission(Permissions.Administrator).ToString()));
                    return;
                }
                db.Messages.Remove(queriedMessage);
                db.SaveChanges();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Deleted entry").AsEphemeral());
            }
        }
        [SlashCommand("DBUpdate", "Update entry in database")]
        public async Task UpdateEntry(InteractionContext ctx,
        [Option("Id", "Message ID")] double DbID,
        [Option("Title", "Updated title")] string? Title = null,
        [Choice("Meme", "Meme")] [Choice("Quote", "Quote")] [Choice("Other", "Other")] [Option("Type", "Updated type")] string? MessageType = null)
        {
            using (var db = new MessageDB())
            {
                var queriedMessage = db.Messages.AsQueryable().Where(x => x.Id == DbID && x.GuildId == ctx.Guild.Id).FirstOrDefault();

                if (queriedMessage == null || (Title == null && MessageType == null))
                {
                    string x="wat";
                    if (ctx.Guild.Id==529624471351590922)
                    {
                        x=":lego_yoda:";
                    }
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(x).AsEphemeral());
                    return;
                }
                if (Title!=null)
                {
                    queriedMessage.Title=Title;
                }
                if (MessageType!=null)
                {
                    queriedMessage.MessageType=MessageType;
                }
                db.Messages.Update(queriedMessage);
                await db.SaveChangesAsync();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Updated entry").AsEphemeral());
            }
        }
        [SlashCommand("DBList", "List entries in database")]
        public async Task ListEntries(InteractionContext ctx,
        [Option("User", "entries from user")] DiscordUser? user = null)
        {
            using (var db = new MessageDB())
            {
                var messages = db.Messages.Where(m=> m.GuildId == ctx.Guild.Id);
                /*
                if (user!=null)
                {
                    messages=messages.Where(m => m.AuthorId == user.Id);
                }*/

                var messageList = messages.ToList();

                if (messageList.Count == 0 )
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No messages found").AsEphemeral());
                    return;
                }


                var idString = string.Join("\n",messages.Select(m => m.Id));
                var titleString = string.Join("\n",messages.Select(m => m.Title ?? "N/A"));
                var typeString = string.Join("\n", messages.Select(m => m.MessageType));
                var authorString = string.Join("\n", messages.Select(m => m.Author));

                var embed = new DiscordEmbedBuilder();

                //Limited to 3 fields inline due to Discord css
                embed.AddField("Id", idString, true);
                embed.AddField("Title", titleString, true);
                embed.AddField("Type", typeString, true);
                //embed.AddField("Author", authorString, true);

                await ctx.CreateResponseAsync(embed.Build());
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
                        DiscordMessageId = message.Id,
                        GuildId = ctx.Guild.Id,
                        Title = Title,
                        Content = messageContent,
                        AttachmentUrls = attList,
                        Author = ctx.User.Username,
                        AuthorId = ctx.User.Id,
                        MessageOriginId = message.Author.Id,
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
                        GuildId = ctx.Guild.Id,
                        Content = quoteContent,
                        Author = ctx.User.Username,
                        AuthorId = ctx.User.Id,
                        MessageOriginId = User.Id,
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
    }
}