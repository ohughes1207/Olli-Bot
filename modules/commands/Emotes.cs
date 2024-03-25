using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
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
                var emoteCounts = new Dictionary<DiscordEmoji, int>();

                var emoteList = ctx.Guild.Emojis.Where(e => e.Value.IsAvailable==true);
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
                            Console.WriteLine(e.Value);
                            var filteredMessages = from m in messages where m.Content.Contains(e.Value) select m;
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
                foreach (var kvp in emoteCounts.OrderByDescending(kv => kv.Value))
                {
                    await ctx.Channel.SendMessageAsync($"Emote: {kvp.Key} | Usage Count: {kvp.Value}");
                    await Task.Delay(400);
                }
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