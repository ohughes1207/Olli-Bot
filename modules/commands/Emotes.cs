using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace self_bot.modules.commands
{
    internal class Emotes : ApplicationCommandModule
    {
        [SlashCommand("Emoterank", "Emote rankings")]
        public async Task RankEmotes(InteractionContext ctx )
        {
            Console.WriteLine(ctx.Guild.Emojis);
        }
    }
}