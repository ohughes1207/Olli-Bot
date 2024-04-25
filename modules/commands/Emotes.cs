using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using self_bot.Migrations;

namespace self_bot.modules.commands
{
    internal class Emotes : ApplicationCommandModule
    {
        [SlashCommand("Emoterank", "Emote rankings")]
        public async Task RankEmotes(InteractionContext ctx)
        {
            try
            {
                //Dictionary of emotes and an integer indicating number of uses 
                var emoteCounts = new Dictionary<DiscordEmoji, int>();

                //only emotes that are available
                var emoteList = ctx.Guild.Emojis.Where(e => e.Value.IsAvailable==true);

                //only text channels
                var channelList = ctx.Guild.Channels.Where(c => c.Value.IsCategory==false && c.Value.Type==0);


                foreach (var ch in channelList)
                {
                    Console.WriteLine(ch.Value.Name);

                    IReadOnlyList<DiscordMessage> messages = null;

                    var lastMessageId = ch.Value.LastMessageId;
                    while (true)
                    {
                        /*if (lastMessageId==null)
                        {
                            break;
                        }*/
                        messages = await ch.Value.GetMessagesBeforeAsync(lastMessageId.Value, 100);
                        Console.WriteLine(lastMessageId);
                        Console.WriteLine(lastMessageId.Value);
                        if (messages.Count == 0)
                        {
                            break;
                        }
                        lastMessageId = messages.Last().Id;
                        foreach (var e in emoteList)
                        {
                            Console.WriteLine(e.Value.GetType());
                            foreach (var m in messages)
                            {
                                if (m.Reactions.Any(r => r.Emoji.Equals(e.Value)))
                                {
                                    Console.WriteLine("CONTAINS REACTION");
                                }
                            } 
                            var filteredMessages = from m in messages where m.Reactions.Any(reaction => reaction.Emoji.Equals(e.Value))/*(m.Content.Contains(e.Value) ||m.Reactions.Any(reaction => reaction.Emoji.Equals(e)))*/ && m.Author.Id!=1118358168708329543 select m;
                            int count = filteredMessages.Count();

                            if (emoteCounts.ContainsKey(e.Value))
                            {
                                emoteCounts[e.Value]+=count;
                            }
                            else
                            {
                                emoteCounts[e.Value]=count;
                            }
                        }
                    }
                }
                // Convert DiscordEmoji to string and calculate the maximum length for alignment
                // int maxKeyLength = emoteCounts.Keys.Select(emoji => emoji.ToString().Length).Max();

                // Create a formatted rank string with aligned data
                var rankString = string.Join("\n", emoteCounts.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key}|{kv.Value}"));

                // Prepare the header and the full message string with proper markdown for code block
                var header = "Emote" + "|Usage Count";
                var messageString = $"{header}\n{rankString}";

                Console.WriteLine(messageString.Length);

                await ctx.Channel.SendMessageAsync(messageString);
                //await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(messageString));
                // Send the formatted string as a single message to the Discord channel
                //await ctx.Channel.SendMessageAsync(messageString);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

/*
            foreach (var e in emoteList)
            {
                await ctx.Channel.SendMessageAsync(e.Value);
                await Task.Delay(400);
            }
        }
    }
}
*/