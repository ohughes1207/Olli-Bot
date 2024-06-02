using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace OlliBot.Modules
{
    public class Emotes : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("emoterank", "Emote rankings")]
        public async Task RankEmotes()
        {
            try
            {
                
                //Dictionary of emotes and an integer indicating number of uses 
                var emoteCounts = new Dictionary<GuildEmote, int>();

                //only emotes that are available
                var emoteList = Context.Guild.Emotes;

                if (emoteList.Count==0)
                {
                    await Context.Interaction.RespondAsync("No emotes found", ephemeral: true);
                    return;
                }

                //only text channels
                var channelList = Context.Guild.Channels.OfType<SocketTextChannel>().Where(ch => ch.GetChannelType() == ChannelType.Text);

                await Context.Interaction.RespondAsync("Bot is working on counting emotes", ephemeral: true);

                foreach (var ch in channelList)
                {
                    Console.WriteLine(ch.Name );


                    IMessage? lastMessage = null;

                    while (true)
                    {
                        var messages = (await (lastMessage == null ? ch.GetMessagesAsync(100).FlattenAsync() : ch.GetMessagesAsync(lastMessage, Direction.Before, 100).FlattenAsync())).ToList()  ;

                        if (messages.Count == 0)
                        {
                            break;
                        }

                        //lastMessageId = messages.Last().Id;

                        lastMessage = messages[messages.Count - 1];

                        foreach (var e in emoteList)
                        {
                            var count = messages.Count(m => (m.Content.Contains(e.ToString()) || m.Reactions.Any(reaction => reaction.Key.Equals(e))) && m.Author.Id!=1118358168708329543);
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
                await Context.Channel.SendMessageAsync(sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}