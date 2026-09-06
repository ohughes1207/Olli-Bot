using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using OlliBot.Application.EmoteRanking.ClearEmoteRanking;
using OlliBot.Application.EmoteRanking.UpdateEmoteRanking;
using System.Text;

namespace OlliBot.Bot.Modules.EmoteRanking;

[RequireContext(ContextType.Guild)]
[Group("emoterank", "Commands for emote ranking")]
public class EmoteRankingSlashCommands(
    ILogger<EmoteRankingSlashCommands> logger,
    IKeyedSemaphore<(ulong, string)> keyedSemaphore,
    ISender sender) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("scan", "Scan emote rankings")]
    public async Task UpdateAndDisplayEmoteRankingsAsync([Summary("Reset")] bool reset = false)
    {
        try
        {
            // Prevents concurrent execution of this command for the same guild
            var key = (Context.Guild.Id, nameof(UpdateAndDisplayEmoteRankingsAsync));
            using IDisposable? lease = keyedSemaphore.TryAcquire(key);

            if (lease is null)
            {
                await RespondAsync("Emote rankings are already being updated for this server.", ephemeral: true);

                return;
            }

            //All custom emotes in a server
            IReadOnlyCollection<GuildEmote> emotes = Context.Guild.Emotes;

            if (reset)
            {
                ClearEmoteRankingResult clearResult = await sender.Send(new ClearEmoteRankingCommand(Context.Guild.Id, ((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.Administrator)));
                if (!clearResult.Success)
                {
                    await Context.Interaction.RespondAsync(clearResult.Message, ephemeral: true);
                    return;
                }
            }

            if (emotes.Count == 0)
            {
                await Context.Interaction.RespondAsync("No emotes found", ephemeral: true);
                return;
            }

            //All channels in a guild that can receive messages
            await Context.Interaction.RespondAsync("Bot is working on counting emotes", ephemeral: true);

            // call the update handler here
            UpdateEmoteRankingResult result = await sender.Send(new UpdateEmoteRankingCommand(Context.Guild.Id));
            if (result.Counts == null || !result.Success)
            {
                await Context.Channel.SendMessageAsync(result.Message);
            }

            if (result.Counts == null || result.Counts.Count == 0)
            {
                await Context.Channel.SendMessageAsync("No emote rankings available.");
                return;
            }

            var formattedRankings = EmoteRankingHelpers.CreateEmoteRankingComponent(result.Counts, emotes);

            await Context.Channel.SendMessageAsync(components: formattedRankings.Build());
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred while handling emote ranking for {GuildId}",
                Context.Guild.Id);
        }
    }

    [SlashCommand("clear", "Clear emote rankings")]
    public async Task ClearEmoteRankingsAsync()
    {
        try
        {
            ClearEmoteRankingResult result = await sender.Send(new ClearEmoteRankingCommand(Context.Guild.Id, ((SocketGuildUser)Context.User).GuildPermissions.Has(GuildPermission.Administrator)));
            await Context.Interaction.RespondAsync(result.Message, ephemeral: true);
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "An error occurred while clearing emote ranking for {GuildId}",
                Context.Guild.Id);
            await Context.Interaction.RespondAsync("An error occurred while clearing emote rankings.", ephemeral: true);
        }
    }

    private static class EmoteRankingHelpers
    {
        internal static ComponentBuilderV2 CreateEmoteRankingComponent(
            IReadOnlyDictionary<ulong, int> emoteCounts,
            IReadOnlyCollection<GuildEmote> guildEmotes)
        {
            var emotesById = guildEmotes.ToDictionary(emote => emote.Id);
            var rankings = new StringBuilder();

            int rank = 1;
            foreach ((ulong emoteId, int count) in emoteCounts.OrderByDescending(entry => entry.Value))
            {
                if (!emotesById.TryGetValue(emoteId, out GuildEmote? emote))
                {
                    // The emote may have been deleted after the scan.
                    continue;
                }

                // GuildEmote.ToString() produces the Discord emote mention.
                rankings.AppendLine($"## {rank}. {emote} - {count}");
            }

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var container = new ContainerBuilder()
                .WithAccentColor(Color.Gold)
                .WithTextDisplay("# ✨Emote Usage Ranking✨")
                .WithTextDisplay($"-# <t:{timestamp}:s>")
                .WithSeparator(spacing: SeparatorSpacingSize.Large)
                .WithTextDisplay(rankings.ToString());

            return new ComponentBuilderV2().WithContainer(container);
        }
    }
}