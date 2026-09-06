using Discord;
using System.Text;
using System.Text.RegularExpressions;

namespace OlliBot.Bot.Utilities;

public static class Helpers
{
    public static bool HasURL(string? input)
    {
        if (string.IsNullOrEmpty(input)) return false;

        var regex = new Regex(@"https?://[^\s/$.?#].[^\s]*");
        return regex.IsMatch(input);
    }

    [Obsolete("Use CreateEmoteRankingComponent")]
    public static string FormatEmoteRankings(
        IReadOnlyDictionary<ulong, int> emoteCounts,
        IReadOnlyCollection<GuildEmote> guildEmotes)
    {
        var emotesById =
            guildEmotes.ToDictionary(
                emote => emote.Id);

        var sb = new StringBuilder();

        sb.AppendLine("Emote Usage Ranking:");

        foreach ((ulong emoteId, int count) in
                 emoteCounts.OrderByDescending(entry => entry.Value))
        {
            if (!emotesById.TryGetValue(emoteId, out GuildEmote? emote))
            {
                // The emote may have been deleted after the scan.
                continue;
            }

            // GuildEmote.ToString() produces the Discord emote mention.
            sb.AppendLine($"{emote} - {count}");
        }

        return sb.ToString();
    }

    public static bool ContentExeedsLength(this IMessage message, int length)
    {
        return message.Content.Length > length;
    }
    public static bool IsAuthorOlliBot(this IMessage message, IDiscordClient discordClient)
    {
        return message.Author.Id == discordClient.CurrentUser.Id;
    }
}
