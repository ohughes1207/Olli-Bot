using Discord;
using OlliBot.Application.HumbleBundle.Models;
using System.Text;

namespace OlliBot.Bot.Modules.HumbleBundle;
internal static class HumbleBundleEmbedBuilder
{
    const string customBulletPoint = "•";
    const string discordBulletPoint = "-";

    static string bulletPoint => discordBulletPoint;

    [Obsolete("Use CreateHumbleBundleComponentV2")]
    internal static Embed CreateHumbleBundleEmbed(ScannedHumbleBundle bundle)
    {
        string description = new StringBuilder()
            .Append($"**Expires:** {TimestampTag.FormatFromDateTime(bundle.ExpiryDate, TimestampTagStyles.ShortDateTime)} ({TimestampTag.FormatFromDateTime(bundle.ExpiryDate, TimestampTagStyles.Relative)})")
            .AppendLine()
            .AppendLine()
            .Append(bundle.ShortDescription)
            .AppendLine()
            .AppendLine()
            .Append($"**{bundle.Note}**")
            .ToString();

        EmbedBuilder embedBuilder = new EmbedBuilder()
            .WithTitle(bundle.Name)
            .WithUrl(bundle.Url)
            .WithImageUrl(bundle.ImageUrl)
            .WithDescription(description)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        foreach (ScannedHumbleBundleTier tier in bundle.BundleTiers.OrderBy(b => b.Tier))
        {
            string items = BuildEmbedFieldText(tier.HumbleBundleItems);


            embedBuilder.AddField(
                $"Tier {tier.Tier} - Pay at least {tier.Price:C}",
                items,
                inline: true);
        }

        return embedBuilder.Build();
    }

    internal static MessageComponent CreateHumbleBundleComponentV2(ScannedHumbleBundle scannedHumbleBundle, string? roleMention = null)
    {
        var builder = new ComponentBuilderV2();

        ContainerBuilder container = CreateHumbleBundleContainerComponent(scannedHumbleBundle, roleMention);

        return builder.WithContainer(container).Build();
    }

    public static ContainerBuilder CreateHumbleBundleContainerComponent(ScannedHumbleBundle scannedHumbleBundle, string? roleMention = null)
    {
        ContainerBuilder container = new ContainerBuilder().WithAccentColor(Color.Blue);

        ActionRowBuilder buttons = new ActionRowBuilder()
            .WithButton(
                label: "View Bundle",
                //customId: $"view_hb_bundle:{scannedHumbleBundle.Url}",
                url: scannedHumbleBundle.Url,
                style: ButtonStyle.Link)
            .WithButton(
                label: "Delete Notification",
                customId: $"delete_hb_notification",
                style: ButtonStyle.Danger,
                emote: new Emoji("🗑️")
                );
        //.WithButton(
        //    label: "Collapse / Expand Bundle",
        //    style: ButtonStyle.Primary,
        //    customId: "expand_bundle");

        var bundleDescription = new StringBuilder($"**Expires:** {TimestampTag.FormatFromDateTime(scannedHumbleBundle.ExpiryDate, TimestampTagStyles.ShortDateTime)} ({TimestampTag.FormatFromDateTime(scannedHumbleBundle.ExpiryDate, TimestampTagStyles.Relative)})");

        if (!string.IsNullOrWhiteSpace(scannedHumbleBundle.ShortDescription))
        {
            bundleDescription.AppendLine();
            bundleDescription.AppendLine();
            bundleDescription.Append(scannedHumbleBundle.ShortDescription);
        }

        if (!string.IsNullOrWhiteSpace(scannedHumbleBundle.Note))
        {
            bundleDescription.AppendLine();
            bundleDescription.AppendLine();
            bundleDescription.Append($"**{scannedHumbleBundle.Note.Trim()}**");
        }

        container
            .WithTextDisplay(new TextDisplayBuilder()
            {
                Content = $"## **[{scannedHumbleBundle.Name}]({scannedHumbleBundle.Url})**"
            })
            .WithSeparator(spacing: SeparatorSpacingSize.Small)
            .WithTextDisplay(new TextDisplayBuilder()
            {
                Content = bundleDescription.ToString()
                //Content = $"**Expires:** {TimestampTag.FormatFromDateTime(scannedHumbleBundle.ExpiryDate, TimestampTagStyles.ShortDateTime)} ({TimestampTag.FormatFromDateTime(scannedHumbleBundle.ExpiryDate, TimestampTagStyles.Relative)})\n\n{scannedHumbleBundle.ShortDescription}\n\n{!string.IsNullOrWhiteSpace(scannedHumbleBundle.Note.Trim()) ? $"**{scannedHumbleBundle.Note.Trim()}**" }",
            })
            .WithSeparator();

        //container.WithSection(new SectionBuilder().WithTextDisplay);

        foreach (ScannedHumbleBundleTier? tier in scannedHumbleBundle.BundleTiers.OrderBy(b => b.Tier))
        {
            container
                .WithTextDisplay(new TextDisplayBuilder
                {
                    Content = $"**Tier {tier.Tier} - Pay at least {tier.Price:C}**"
                });

            string items = BuildEmbedFieldText(tier.HumbleBundleItems);

            // This check is required because recently a bundle was released that had a 2nd tier
            // where the only difference is that you get a 2nd copy of the same games in tier 1
            // So the 2nd tier had no *new* items
            container.WithTextDisplay(new TextDisplayBuilder
            {
                Content = !string.IsNullOrWhiteSpace(items) ? items : $"{bulletPoint} No new items"
            });

            //container.WithSection(tierSection);
            container.WithSeparator();
            //if (tier != scannedHumbleBundle.BundleTiers.OrderBy(b => b.Tier).Last())
            //{
            //    container.WithSeparator();
            //}
        }

        container
            .WithMediaGallery(new MediaGalleryBuilder().AddItem(new MediaGalleryItemProperties
            {
                Media = new UnfurledMediaItemProperties
                {
                    Url = scannedHumbleBundle.ImageUrl
                },
                //Description = "Bundle Thumbnail"
            }));

        container.WithActionRow(buttons);

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var mention = !string.IsNullOrWhiteSpace(roleMention)
            ? $" {roleMention}"
            : string.Empty;

        container.WithTextDisplay($"-# <t:{timestamp}:R>{mention}");

        return container;
    }

    private static string BuildEmbedFieldText(List<ScannedHumbleBundleItem> items)
    {
        const int maxLength = 1024;
        string moreText = $"{bulletPoint} And more...";

        var sb = new StringBuilder();

        for (int i = 0; i < items.Count; i++)
        {
            ScannedHumbleBundleItem item = items[i];

            string itemText = string.IsNullOrWhiteSpace(item.ExtraInfo)
                ? $"{bulletPoint} {item.ItemName}"
                : $"{bulletPoint} {item.ItemName} *({item.ExtraInfo})*";

            bool isLastItem = i == items.Count - 1;

            int requiredLength =
                itemText.Length +
                Environment.NewLine.Length;

            if (!isLastItem)
            {
                requiredLength += moreText.Length;
            }

            if (sb.Length + requiredLength > maxLength)
            {
                sb.Append(moreText);
                break;
            }

            sb.AppendLine(itemText);
        }

        return sb.ToString();
    }
}
