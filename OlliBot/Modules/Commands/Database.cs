using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OlliBot.Data;


namespace OlliBot.Modules
{
    public class DatabaseCommands : InteractionModuleBase<SocketInteractionContext>
    {
        //Command to add entries to database
        [SlashCommand("dbadd", "Add a discord message to the database")]
        public async Task AddMessage([Summary("message", "Enter a message ID or quote content")] string messageEntry,
        [Summary("title", "Title (Optional)")] string? Title = null,
        [Summary("origin", "Quote origin (Optional if using Message ID for input)")] SocketGuildUser? User = null,
        [Choice("Meme", "Meme")] [Choice("Quote", "Quote")] [Choice("Other", "Other")] [Summary("Type", "Type (If no value set then will be implicitly determined)")] string? MessageType = null)
        {            
            //If input for message is not convertable to ulong assume it is a manually entered quote
            if (!ulong.TryParse(messageEntry, out ulong result))
            {
                //If manually entered quote has no origin then respond with unsuccessful message
                if (User is null)
                {
                    await Context.Interaction.RespondAsync("Entry unsuccessful, try again with a quote origin.", ephemeral: true);
                    return;
                }
                //Call method to add manually entered quotes
                await DatabaseLogic.AddQuoteManual(Context, messageEntry, User);
                return;
            }
            //If input for message is ulong then call method to add method to database from it's Id
            await DatabaseLogic.AddByID(Context, messageEntry, Title, MessageType);
        }
        //Command to call an entry from the database based on ID
        [SlashCommand("db", "Call entry by ID from the database")]
        public async Task CallMessage([Summary("Query", "Message ID or Title")] string query)
        {
            using (var db = new MessageDB())
            {
                var guildMessages = db.Messages.AsQueryable().Where(x=> x.GuildId == Context.Guild.Id);
                
                Message? queriedMessage;

                if (int.TryParse(query, out int intQuery))
                {
                    queriedMessage = guildMessages.Where(x => x.Id == intQuery).FirstOrDefault();
                }
                else
                {
                    queriedMessage = guildMessages.Where(x => x.Title != null && x.Title.ToLower().Contains(query.ToLower())).FirstOrDefault();

                }

                //var responseBuilder = new DiscordInteractionResponseBuilder();


                if (queriedMessage is null)
                {
                    await Context.Interaction.RespondAsync("No message found", ephemeral: true);
                    //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder.WithContent("No message found").AsEphemeral());
                    return;
                }


                if (queriedMessage.DiscordMessageId is null && queriedMessage.MessageType=="Quote")
                {
                    IGuildUser quoteOrigin = Context.Guild.GetUser(queriedMessage.MessageOriginId);
                    string responseContent = $"\"{queriedMessage.Content}\" - {quoteOrigin.DisplayName}";
                    await Context.Interaction.RespondAsync(responseContent);
                    //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder.WithContent(responseContent));
                }
                else
                {

                    string responseContent = queriedMessage.Content ?? string.Empty;
                    
                    if (queriedMessage.AttachmentUrls.Count > 0)
                    {
                        responseContent+=Environment.NewLine;
                        foreach (var attachment in queriedMessage.AttachmentUrls)
                        {
                            responseContent+=attachment;
                        }
                    }

                    await Context.Interaction.RespondAsync(responseContent);
                    //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder.WithContent(responseContent));
                }
            }

        }
        [SlashCommand("dbdelete", "Delete an entry from the database")]
        public async Task DeleteEntry([Summary("Id", "Database ID")] double DbID)
        {
            SocketGuildUser user = (SocketGuildUser)Context.User;

            using (var db = new MessageDB())
            {
                Message? queriedMessage = db.Messages.AsQueryable().Where(x => x.Id == DbID && x.GuildId == Context.Guild.Id).FirstOrDefault();

                if (queriedMessage is null)
                {
                    await Context.Interaction.RespondAsync("No Entry found", ephemeral: true);
                    //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No Entry found").AsEphemeral());
                    return;
                }

                if (queriedMessage.AuthorId!=Context.User.Id && !user.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    await Context.Interaction.RespondAsync(user.GuildPermissions.Has(GuildPermission.Administrator).ToString());
                    //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(ctx.Member.Permissions.HasPermission(Permissions.Administrator).ToString()));
                    return;
                }
                db.Messages.Remove(queriedMessage);
                db.SaveChanges();

                await Context.Interaction.RespondAsync("Deleted entry", ephemeral: true);

                //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Deleted entry").AsEphemeral());
            }
        }
        [SlashCommand("dbupdate", "Update entry in database")]
        public async Task UpdateEntry([Summary("Id", "Message ID")] double DbID,
        [Summary("Title", "Updated title")] string? Title = null,
        [Choice("Meme", "Meme")] [Choice("Quote", "Quote")] [Choice("Other", "Other")] [Summary("Type", "Updated type")] string? MessageType = null)
        {
            using (var db = new MessageDB())
            {
                var queriedMessage = db.Messages.AsQueryable().Where(x => x.Id == DbID && x.GuildId == Context.Guild.Id).FirstOrDefault();

                if (queriedMessage == null || (Title == null && MessageType == null))
                {
                    string x="wat";
                    if (Context.Guild.Id==529624471351590922)
                    {
                        x=":lego_yoda:";
                    }
                    await Context.Interaction.RespondAsync(x, ephemeral: true);
                    //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(x).AsEphemeral());
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
                await Context.Interaction.RespondAsync("Updated entry", ephemeral: true);
                //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Updated entry").AsEphemeral());
            }
        }
        [SlashCommand("dblist", "List entries in database")]
        public async Task ListEntries([Summary("User", "entries from user")] SocketUser? user = null)
        {
            using (var db = new MessageDB())
            {
                var messages = db.Messages.Where(m=> m.GuildId == Context.Guild.Id);
                /*
                if (user!=null)
                {
                    messages=messages.Where(m => m.AuthorId == user.Id);
                }*/

                var messageList = messages.ToList();

                if (messageList.Count == 0 )
                {
                    await Context.Interaction.RespondAsync("No messages found", ephemeral: true);
                    //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No messages found").AsEphemeral());
                    return;
                }


                var idString = string.Join("\n",messages.Select(m => m.Id));
                var titleString = string.Join("\n",messages.Select(m => m.Title?? "N/A"));
                var typeString = string.Join("\n", messages.Select(m => m.MessageType));
                var authorString = string.Join("\n", messages.Select(m => m.Author));

                var embed = new EmbedBuilder();

                //Limited to 3 fields inline due to Discord css
                embed.AddField("Id", idString, true);
                embed.AddField("Title", titleString, true);
                embed.AddField("Type", typeString, true);
                embed.WithColor(Color.Gold);
                embed.WithTitle("Olli Bot Database");

                //embed.AddField("Author", authorString, true);
                await Context.Interaction.RespondAsync(embed: embed.Build());
                //await ctx.CreateResponseAsync(embed.Build());
                //await ctx.CreateResponseAsync(embed.Build(), true);
            }
        }
    }

    internal class DatabaseLogic
    {
        internal static string GetMessageType(IMessage message)
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
        internal static async Task AddByID(SocketInteractionContext ctx, string messageID, string? Title, string? MessageType)
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
                //Embed is a image/video embedded from a link within the message content
                if (message.Embeds.Count > 0)
                {
                    foreach (var embed in message.Embeds)
                    {
                        attList.Add(embed.Url);
                        messageContent = messageContent.Replace(embed.Url, "");
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

                    await ctx.Interaction.RespondAsync("Entry added to the database", ephemeral: true);
                    //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Entry added to the database").AsEphemeral());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        
        internal static async Task AddQuoteManual(SocketInteractionContext ctx, string quoteContent, SocketUser User)
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

                    await ctx.Interaction.RespondAsync("Entry added to the database", ephemeral: true);
                    //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Entry added to the database").AsEphemeral());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}