using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using OlliBot.Application.HumbleBundle.AddHumbleBundleSubscriber;
using OlliBot.Application.HumbleBundle.GetLatestHumbleBundle;
using OlliBot.Application.HumbleBundle.GetUserHumbleBundleSubscriptions;
using OlliBot.Application.HumbleBundle.Models;
using OlliBot.Application.HumbleBundle.RemoveHumbleBundleSubscriber;
using OlliBot.Application.HumbleBundle.ScanHumbleBundle;
using OlliBot.Application.HumbleBundle.UpdateSubscriptionRole;
using OlliBot.Domain.Enums;

namespace OlliBot.Bot.Modules.HumbleBundle;

[Group("hb", "Humble bundle commands")]
public class HumbleBundleSlashCommands(
    ISender sender) : InteractionModuleBase<SocketInteractionContext>
{
    #region Slash Commands
    [SlashCommand("all", "Get all Humble Bundles of a specific type")]
    [RequireDmOrGuildPermission(GuildPermission.Administrator)]
    public async Task GetHumbleBundles([Summary("Type")] HumbleBundleType humbleBundleType)
    {
        await RespondAsync("Retrieving Humble Bundles...", ephemeral: true);
        // Get humble bundles
        ScanHumbleBundleResult result = await sender.Send(new ScanHumbleBundleCommand(humbleBundleType));

        // Build embeds for each bundle 
        // Send found humbles to user / text channel
        foreach (ScannedHumbleBundle bundle in result.ScannedBundles)
        {
            await Context.Channel.SendMessageAsync(components: HumbleBundleEmbedBuilder.CreateHumbleBundleComponentV2(bundle));
        }
    }

    [SlashCommand("latest", "Get the latest Humble Bundle of a specific type")]
    public async Task GetLatestHumbleBundle([Summary("Type")] HumbleBundleType humbleBundleType)
    {
        await RespondAsync("Retrieving latest Humble Bundle...", ephemeral: true);

        // Get humble bundles
        GetLatestHumbleBundleResult result = await sender.Send(new GetLatestHumbleBundleQuery(humbleBundleType));
        if (result.Bundle == null)
        {
            await Context.Channel.SendMessageAsync("No Humble Bundle found for the specified type.");
            return;
        }
        await Context.Channel.SendMessageAsync(components: HumbleBundleEmbedBuilder.CreateHumbleBundleComponentV2(result.Bundle));
    }

    // How should we handled this?
    // Should we simply handle calling this method in a guild differently from in a DM?
    // 
    [SlashCommand("subscribe", "Subscribe for Humble Bundle updates")]
    [RequireDmOrGuildPermission(GuildPermission.Administrator)]
    public async Task ManageSubscriptions()
    {
        var subsriberId = Context.Guild != null ? Context.Channel.Id : Context.User.Id;
        GetUserHumbleBundleSubscriptionsResult subscriptions = await sender.Send(new GetUserHumbleBundleSubscriptionsQuery(subsriberId));

        MessageComponent components = BuildSubscriptionManagerComponents(subscriptions.HumbleBundleTypes, subscriptions.RoleId);

        await RespondAsync(
            components: components,
            flags: MessageFlags.Ephemeral | MessageFlags.ComponentsV2);
    }
    #endregion

    #region Component Interactions

    [ComponentInteraction("bundle_alert_role:*", ignoreGroupNames: true)]
    public async Task HandleRoleAsync(string _, IRole[] roles)
    {
        var component = (SocketMessageComponent)Context.Interaction;
        var selectedRole = roles.FirstOrDefault();

        var subscriptionsResult =
            await sender.Send(
                new GetUserHumbleBundleSubscriptionsQuery(Context.Channel.Id));

        bool hasSubscriptions = subscriptionsResult.HumbleBundleTypes.Count > 0;

        // User cleared the role selection
        if (selectedRole is null)
        {
            if (hasSubscriptions)
            {
                await sender.Send(
                    new UpdateSubscriptionRoleCommand(
                        Context.Channel.Id,
                        null));
            }

            await component.UpdateAsync(properties =>
            {
                properties.Components = BuildSubscriptionManagerComponents(
                    subscriptionsResult.HumbleBundleTypes,
                    null);
            });

            await FollowupAsync(
                "Removed role from being pinged",
                ephemeral: true);

            return;
        }

        // Administrator roles aren't allowed
        if (selectedRole.Permissions.Has(GuildPermission.Administrator))
        {
            await component.UpdateAsync(properties =>
            {
                properties.Components = BuildSubscriptionManagerComponents(
                    subscriptionsResult.HumbleBundleTypes,
                    null);
            });

            await FollowupAsync(
                "Administrator roles cannot be used for bundle alerts.",
                ephemeral: true);

            return;
        }

        // Valid role
        ulong roleId = selectedRole.Id;

        if (hasSubscriptions)
        {
            await sender.Send(
                new UpdateSubscriptionRoleCommand(
                    Context.Channel.Id,
                    roleId));
        }

        await component.UpdateAsync(properties =>
        {
            properties.Components = BuildSubscriptionManagerComponents(
                subscriptionsResult.HumbleBundleTypes,
                roleId);
        });

        if (hasSubscriptions)
        {
            await FollowupAsync(
                $"Updated subscriptions to ping {selectedRole.Mention}.",
                ephemeral: true);
        }
    }

    [ComponentInteraction("bundle_types:*", ignoreGroupNames: true)]
    public async Task UpdateBundleSubscriptionsAsync(string roleId, string[] bundleTypesString)
    {
        ulong? selectedRoleId = ulong.TryParse(roleId, out ulong parsedRoleId)
            ? parsedRoleId
            : null;

        HumbleBundleType[] selectedBundleTypes = bundleTypesString
            .Select(x => Enum.Parse<HumbleBundleType>(x, ignoreCase: true))
            .ToArray();

        ulong subscriberId = Context.Guild != null ? Context.Channel.Id : Context.User.Id;

        var currentSubscriptions = await sender.Send(new GetUserHumbleBundleSubscriptionsQuery(subscriberId));

        HumbleBundleType[] subscriptionsToAdd = selectedBundleTypes
            .Except(currentSubscriptions.HumbleBundleTypes)
            .ToArray();

        HumbleBundleType[] subscriptionsToRemove = currentSubscriptions.HumbleBundleTypes
            .Except(selectedBundleTypes)
            .ToArray();

        var messages = new List<string>();

        foreach (HumbleBundleType bundleType in subscriptionsToAdd)
        {
            AddHumbleBundleSubscriberCommand command;

            

            if (Context.Guild != null)
            {
                command = new AddHumbleBundleSubscriberCommand(
                    bundleType,
                    subscriberId,
                    HumbleBundleSubscriberType.Channel,
                    Context.Guild.Id,
                    selectedRoleId);
            }
            else
            {
                command = new AddHumbleBundleSubscriberCommand(
                    bundleType,
                    subscriberId,
                    HumbleBundleSubscriberType.User);
            }

            AddHumbleBundleSubscriberResult result = await sender.Send(command);

            if (result.Success)
            {
                messages.Add($"Subscribed to {bundleType} Humble Bundle updates.");
            }
            else
            {
                messages.Add(
                    $"Failed to subscribe to {bundleType}: {result.Message}");
            }
        }

        foreach (HumbleBundleType bundleType in subscriptionsToRemove)
        {
            RemoveHumbleBundleSubscriberCommand command;

            if (Context.Guild != null)
            {
                command = new RemoveHumbleBundleSubscriberCommand(
                    bundleType,
                    subscriberId,
                    HumbleBundleSubscriberType.Channel);
            }
            else
            {
                command = new RemoveHumbleBundleSubscriberCommand(
                    bundleType,
                    subscriberId,
                    HumbleBundleSubscriberType.User);

            }

            RemoveHumbleBundleSubscriberResult result = await sender.Send(command);

            if (result.Success)
            {
                messages.Add($"Unsubscribed from {bundleType} Humble Bundle updates.");
            }
            else
            {
                messages.Add($"Failed to unsubscribe from {bundleType}: {result.Message}");
            }
        }

        await RespondAsync(
            string.Join(Environment.NewLine, messages),
            ephemeral: true);
    }

    [ComponentInteraction("delete_hb_notification", ignoreGroupNames: true)]
    [RequireDmOrGuildPermission(GuildPermission.Administrator)]
    public async Task DeleteNotification()
    {
        var componentInteraction = (SocketMessageComponent)Context.Interaction;

        SocketUserMessage message = componentInteraction.Message;

        await message.DeleteAsync();
    }

    #endregion

    private MessageComponent BuildSubscriptionManagerComponents(
        IReadOnlyCollection<HumbleBundleType> subscriptionList,
        ulong? roleId)
    {
        var componentsBuilder = new ComponentBuilderV2()
            .WithTextDisplay(
                $"### Select bundle types to subscribe{(Context.Guild != null ? " this channel" : "")} to");

        if (Context.Guild != null)
        {
            // Use a guid in the custom id so that discord clears the select menu when an invalid role is selected
            var roleSelect = new SelectMenuBuilder()
                .WithCustomId($"bundle_alert_role:{Guid.NewGuid()}")
                .WithMinValues(0)
                .WithMaxValues(1)
                .WithType(ComponentType.RoleSelect);

            if (roleId.HasValue)
            {
                roleSelect.WithDefaultValues(
                    new SelectMenuDefaultValue(
                        roleId.Value,
                        SelectDefaultValueType.Role));
            }
            else
            {
                roleSelect.DefaultValues.Clear();
            }

            componentsBuilder.WithActionRow(
                new ActionRowBuilder()
                    .WithSelectMenu(roleSelect));
        }

        var typeSelect = new SelectMenuBuilder()
            .WithCustomId(roleId.HasValue ? $"bundle_types:{roleId.Value}" : "bundle_types:none")
            .WithMinValues(0)
            .WithMaxValues(3)
            .AddOption(
                HumbleBundleType.Games.ToString(),
                "games",
                isDefault: subscriptionList.Contains(HumbleBundleType.Games))
            .AddOption(
                HumbleBundleType.Software.ToString(),
                "software",
                isDefault: subscriptionList.Contains(HumbleBundleType.Software))
            .AddOption(
                HumbleBundleType.Books.ToString(),
                "books",
                isDefault: subscriptionList.Contains(HumbleBundleType.Books));

        componentsBuilder.WithActionRow(
            new ActionRowBuilder()
                .WithSelectMenu(typeSelect));

        return componentsBuilder.Build();
    }
}
