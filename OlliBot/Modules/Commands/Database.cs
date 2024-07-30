using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OlliBot.Data;
using OlliBot.Utilities;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace OlliBot.Modules
{
    public class DatabaseCommands : InteractionModuleBase<SocketInteractionContext>
    {
        //Command to add entries to database
        [SlashCommand("dbadd", "Add a discord message to the database")]
        public async Task AddMessage([Summary("message", "Enter a message ID or quote content")] string messageEntry,
        [Summary("title", "Title (Optional)")] string? Title = null,
        [Summary("origin", "Quote origin (Optional if using Message ID for input)")] SocketGuildUser? User = null,
        [Choice("Meme", "Meme")] [Choice("Quote", "Quote")] [Choice("Other", "Other")] [Summary("Type", "Type (If no value set then will be implicitly determined)")] string? messageType = null)
        {
            Message entry;

            if (ulong.TryParse(messageEntry, out ulong result))
            {
                var DiscordMessageObj = await Context.Interaction.Channel.GetMessageAsync(result);
                entry = DatabaseLogic.CreateMessageFromInput(DiscordMessageObj, Title, Context, messageType);
            }
            //If input for message is not convertable to ulong assume it is a manually entered quote
            else
            {
                //If manually entered quote has no origin then respond with unsuccessful message
                if (User is null)
                {
                    await Context.Interaction.RespondAsync("Entry unsuccessful, try again with a quote origin.", ephemeral: true);
                    return;
                }
                entry = DatabaseLogic.CreateMessageFromInput(messageEntry, Title, Context, messageType, User.Id);
                //Call method to add manually entered quotes
                //await DatabaseLogic.AddQuoteManual(Context, messageEntry, User);
                //return;
            }

            entry.MessageType = messageType ?? DatabaseLogic.GetMessageType(entry);

            if (!string.IsNullOrEmpty(entry.Content) && Helpers.HasURL(entry.Content))
            {
                // Add logic here
                var regex = new Regex(@"https?://[^\s/$.?#].[^\s]*");
                var matches = regex.Matches(entry.Content).Select(m => m.Value).ToList();

                entry.AttachmentUrls.AddRange(matches);

                entry.Content = regex.Replace(entry.Content, string.Empty);
            }

            using (var db = new MessageDB())
            {

                await db.Messages.AddAsync(entry);
                await db.SaveChangesAsync();

                await Context.Interaction.RespondAsync("Entry added to the database", ephemeral: true);
            }
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
                    await Context.Interaction.RespondAsync(x, ephemeral: true);
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
            }
        }
        [SlashCommand("dblist", "List entries in database")]
        public async Task ListEntries([Summary("User", "entries from user")] SocketUser? user = null)
        {
            using (var db = new MessageDB())
            {
                var messages = db.Messages.Where(m=> m.GuildId == Context.Guild.Id);
                
                if (user != null)
                {
                    messages=messages.Where(m => m.AuthorId == user.Id);
                }

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
                await Context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
                //await ctx.CreateResponseAsync(embed.Build());
                //await ctx.CreateResponseAsync(embed.Build(), true);
            }
        }
    }

    internal class DatabaseLogic
    {
        internal static Message CreateMessageFromInput(IMessage message, string? Title, SocketInteractionContext ctx, string? messageType)
        {
            var attList = new List<string>();

            string entryContent = message.Content;

            //Attachment is a file upload attached to a message
            if (message.Attachments.Count > 0)
            {
                foreach (var attachment in message.Attachments)
                {
                    attList.Add(attachment.Url);
                }
            }

            var regex = new Regex(@"(http|https):\/\/[^\s/$.?#].[^\s]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Find all URLs in entryContent
            var matches = regex.Matches(entryContent);
            foreach (Match match in matches)
            {
                attList.Add(match.Value);
            }

            entryContent = regex.Replace(entryContent, string.Empty);


            var entry = new Message
            {
                DiscordMessageId = message.Id,
                GuildId = ctx.Guild.Id,
                Title = Title,
                Content = entryContent,
                AttachmentUrls = attList,
                Author = ctx.User.Username,
                AuthorId = ctx.User.Id,
                MessageOriginId = message.Author.Id,
                DateTimeAdded = DateTime.UtcNow
            };

            return entry;
        }
        internal static Message CreateMessageFromInput(string entryContent, string? Title, SocketInteractionContext ctx, string? messageType, ulong originId)
        {
            var attList = new List<string>();

            // CHECK IF entryContent is url here
            var regex = new Regex(@"(http|https):\/\/[^\s/$.?#].[^\s]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Find all URLs in entryContent
            var matches = regex.Matches(entryContent);
            foreach (Match match in matches)
            {
                attList.Add(match.Value);
            }

            entryContent = regex.Replace(entryContent, string.Empty);

            var entry = new Message
            {
                GuildId = ctx.Guild.Id,
                Title = Title,
                Content = entryContent,
                AttachmentUrls = attList,
                Author = ctx.User.Username,
                AuthorId = ctx.User.Id,
                MessageOriginId = originId,
                DateTimeAdded = DateTime.UtcNow
            };

            return entry;
        }
        internal static string GetMessageType(Message message)
        {
            if (message.AttachmentUrls.Count > 0)
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
    }
}