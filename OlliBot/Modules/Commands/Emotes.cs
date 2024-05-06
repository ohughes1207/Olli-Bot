using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Attributes;

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
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Bot is working on counting emotes").AsEphemeral());
                
                //Dictionary of emotes and an integer indicating number of uses 
                var emoteCounts = new Dictionary<DiscordEmoji, int>();

                //only emotes that are available
                var emoteList = ctx.Guild.Emojis.Where(e => e.Value.IsAvailable==true);

                //only text channels
                var channelList = ctx.Guild.Channels.Where(c => c.Value.IsCategory==false && c.Value.Type==0);


                foreach (var ch in channelList)
                {
                    Console.WriteLine(ch.Value.Name);

                    ulong? lastMessageId = null;
                    while (true)
                    {
                        IReadOnlyList<DiscordMessage> messages = lastMessageId == null ? await ch.Value.GetMessagesAsync(100) : await ch.Value.GetMessagesBeforeAsync(lastMessageId.Value, 100);

                        if (messages.Count == 0)
                        {
                            break;
                        }
                        lastMessageId = messages.Last().Id;
                        foreach (var e in emoteList)
                        {
                            var filteredMessages = from m in messages where (m.Content.Contains(e.Value) || m.Reactions.Any(reaction => reaction.Emoji.Equals(e.Value))) && m.Author.Id!=1118358168708329543 select m;
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

                // Create a formatted rank string with aligned data
                var rankString = string.Join("\n", emoteCounts.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key}  -  {kv.Value}"));

                var header = "Emote Usage Ranking:";
                var messageString = $"{header}\n{rankString}";


                Console.WriteLine(messageString.Length);

                // Send the formatted string as a single message to the Discord channel
                await ctx.Channel.SendMessageAsync(messageString);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}