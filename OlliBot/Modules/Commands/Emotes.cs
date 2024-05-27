using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Attributes;
using System.Text;

namespace OlliBot.Modules
{
    internal class Emotes : ApplicationCommandModule
    {
        [SlashCommand("Emoterank", "Emote rankings")]
        [SlashCooldown(1, 600, SlashCooldownBucketType.Guild)]
        public async Task RankEmotes(InteractionContext ctx)
        {
            try
            {
                
                //Dictionary of emotes and an integer indicating number of uses 
                var emoteCounts = new Dictionary<DiscordEmoji, int>();

                //only emotes that are available
                var emoteList = ctx.Guild.Emojis.Where(e => e.Value.IsAvailable);

                if (emoteList.Count()==0)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No emotes found").AsEphemeral());
                    return;
                }

                //only text channels
                var channelList = ctx.Guild.Channels.Where(c => !c.Value.IsCategory && c.Value.Type==0);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Bot is working on counting emotes").AsEphemeral());

                foreach (var ch in channelList.Select(c => c.Value))
                {

                    ulong? lastMessageId = null;

                    while (true)
                    {
                        IReadOnlyList<DiscordMessage> messages = lastMessageId == null ? await ch.GetMessagesAsync(100) : await ch.GetMessagesBeforeAsync(lastMessageId.Value, 100);

                        if (messages.Count == 0)
                        {
                            break;
                        }

                        //lastMessageId = messages.Last().Id;

                        lastMessageId = messages[messages.Count - 1].Id;

                        foreach (var e in emoteList.Select(e => e.Value))
                        {
                            var count = messages.Count(m => (m.Content.Contains(e) || m.Reactions.Any(reaction => reaction.Emoji.Equals(e))) && m.Author.Id!=1118358168708329543);
                            //int count = filteredMessages.Count();

                            if (emoteCounts.ContainsKey(e))
                            {
                                emoteCounts[e]+=count;
                            }
                            else
                            {
                                emoteCounts[e]=count;
                            }
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Emote Usage Ranking:");

                foreach (var kv in emoteCounts.OrderByDescending(kv => kv.Value))
                {
                    sb.AppendLine($"{kv.Key}  -  {kv.Value}");
                }

                /*
                var rankString = string.Join("\n", emoteCounts.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key}  -  {kv.Value}"));

                var header = "Emote Usage Ranking:";
                var messageString = $"{header}\n{rankString}";
                */

                // Send the formatted string as a single message to the Discord channel
                await ctx.Channel.SendMessageAsync(sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}