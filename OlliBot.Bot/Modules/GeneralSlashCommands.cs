using Discord;
using Discord.Interactions;

namespace OlliBot.Bot.Modules;

public class GeneralSlashCommands(ILogger<GeneralSlashCommands> logger, IConfiguration configuration, IDiscordClient discordClient) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly string[] GifCollection =
    [
        "https://c.tenor.com/7xrOS-GaGAIAAAAd/tenor.gif",//
        "https://c.tenor.com/XvgNEZWCQwgAAAAd/tenor.gif",
        "https://c.tenor.com/ozunReUPzCMAAAAd/tenor.gif",
        "https://c.tenor.com/5Epx4bEKJA4AAAAd/tenor.gif",
        "https://c.tenor.com/8FOQORmaLNoAAAAd/tenor.gif",
        "https://c.tenor.com/prf8e9xuagwAAAAd/tenor.gif",
        "https://c.tenor.com/rtHwrLRPlAkAAAAd/tenor.gif",
        "https://c.tenor.com/1vt_6_y0nQsAAAAd/tenor.gif",
        "https://c.tenor.com/CQfnJKBouuIAAAAd/tenor.gif",
        "https://c.tenor.com/5a4O1hOHucgAAAAd/tenor.gif",
        "https://c.tenor.com/2IXwqmUciHAAAAAd/tenor.gif",
        "https://c.tenor.com/GqIs6RMWlQ4AAAAd/tenor.gif",
        "https://c.tenor.com/rolIhVHxETIAAAAd/tenor.gif",
        "https://c.tenor.com/N41zKEDABuUAAAAd/tenor.gif",
        "https://c.tenor.com/-hkJYNs7tUkAAAAd/tenor.gif",
        "https://c.tenor.com/YMRmKEdwZCgAAAAd/tenor.gif",
        "https://c.tenor.com/wLqFGYigJuIAAAAd/tenor.gif",
        "https://c.tenor.com/XrFi4FThPFYAAAAd/tenor.gif",
        "https://c.tenor.com/Vw4wf7gsD4cAAAAd/tenor.gif",
        "https://c.tenor.com/OvrmH29V-44AAAAd/tenor.gif",
        "https://c.tenor.com/7xrOS-GaGAIAAAAd/tenor.gif",
        "https://c.tenor.com/OGnRVWCps7IAAAAd/tenor.gif"
    ];

    [SlashCommand("avatar", "Get the avatar of the specified user")]
    public async Task Avatar([Summary("user", "Specified user")] IUser? user = null)
    {
        user ??= Context.User;

        string displayName = user switch
        {
            IGuildUser guildUser => guildUser.Nickname
                                 ?? guildUser.DisplayName
                                 ?? guildUser.Username,
            _ => user.Username
        };

        var embed = new EmbedBuilder();
        embed.WithTitle($"{displayName}'s avatar");
        embed.WithColor(new Color(252, 177, 3));
        embed.WithUrl(user.GetAvatarUrl(ImageFormat.Png, 1024));
        embed.WithImageUrl(user.GetAvatarUrl(ImageFormat.Png, 1024));

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("info", "Get server info for a user")]
    [RequireContext(ContextType.Guild)]
    public async Task ServerInfo([Summary("user", "Specified user")] IGuildUser? member = null)
    {
        member ??= (IGuildUser)Context.User;

        string nickname = member.Nickname ?? member.DisplayName ?? member.Username;

        var embed = new EmbedBuilder();

        embed.WithTitle($"{nickname} ({member.Username}) server info");
        embed.AddField("Account Created:", $"<t:{member.CreatedAt.ToUnixTimeSeconds()}:s>", true);
        embed.AddField("Join date:", $"<t:{member.JoinedAt?.ToUnixTimeSeconds()}:s>", true);
        //embed.AddField("Current Activity:", $"{user.Presence.Activity.ActivityType. ?? "Nothing"}", true);
        //embed.WithColor(DiscordColor.Orange);
        embed.WithColor(new Color(252, 177, 3));
        embed.WithThumbnailUrl(member.GetAvatarUrl(ImageFormat.Auto, 1024));

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("coinflip", "Flip a coin")]
    public async Task CoinFlip()
    {
        int rng = new Random().Next(1, 101);

        string result = rng > 50 ? "Heads!" : "Tails!";

        await RespondAsync(result);
    }

    [SlashCommand("headpat", "give headpats")]
    [RequireContext(ContextType.Guild | ContextType.Group)]
    public async Task CreateHeadPatButton()
    {
        const ulong myId = 119904333750861824;
        var userId = configuration.GetValue<ulong>("OwnerID", myId);

        var builder = new ComponentBuilderV2();

        //ContainerBuilder container = new ContainerBuilder().WithAccentColor(Color.Magenta);

        builder.WithActionRow(new ActionRowBuilder().WithButton(
            label: $"Give Olli headpats",
            customId: $"headpat:{userId}",
            style: ButtonStyle.Primary,
            emote: new Emoji("😻")));

        //builder.WithContainer(container);

        await Context.Interaction.RespondAsync(components: builder.Build());
    }

    [ComponentInteraction("headpat:*")]
    public async Task HeadPat(string userId)
    {
        int randomIndex = Random.Shared.Next(GifCollection.Length);
        var selectedGif = GifCollection[randomIndex];

        //string? gifUrl = await gifProvider.GetRandomGifAsync("anime headpat", ct);

        //var embed = new EmbedBuilder()
        //    .WithTitle($"<@{Context.User.Id}> *headpats* <@{userId}> 🥺")
        //    .WithImageUrl(selectedGif)
        //    .WithColor(Color.Magenta)
        //    .Build();

        logger.LogInformation("Selected GIF: {selectedGif}", selectedGif);

        var container = new ContainerBuilder().WithAccentColor(Color.Magenta)
            .WithTextDisplay($"# <@{Context.User.Id}> *headpats* <@{userId}> \U0001f97a")
            .WithMediaGallery(new MediaGalleryBuilder().AddItem(new MediaGalleryItemProperties
            {
                Media = new UnfurledMediaItemProperties
                {
                    Url = selectedGif,
                }
            }))
            .WithActionRow(new ActionRowBuilder().WithButton(
                label: $"Give Olli headpats",
                customId: $"headpat:{userId}",
                style: ButtonStyle.Primary,
                emote: new Emoji("😻")));

        await Context.Interaction.RespondAsync(components: new ComponentBuilderV2().WithContainer(container).Build(), allowedMentions: AllowedMentions.None);
    }

    [SlashCommand("help", "Get a list of available commands")]
    public async Task Help()
    {
        try
        {
            // create a component v2 message with buttons for each command
            throw new NotImplementedException("Help command is not implemented yet.");
        }
        catch (Exception ex)
        {
            await Context.Interaction.RespondAsync(ex.Message, ephemeral: true);
            logger.LogError(ex, "Failed to process command");
        }
    }
}