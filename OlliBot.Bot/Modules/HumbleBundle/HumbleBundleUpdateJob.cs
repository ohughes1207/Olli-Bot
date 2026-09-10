using Discord;
using MediatR;
using OlliBot.Application.HumbleBundle.CheckForHumbleBundleUpdates;
using OlliBot.Application.HumbleBundle.Models;
using OlliBot.Domain.Entities;
using OlliBot.Domain.Enums;
using Quartz;
using System.ComponentModel;
using System.Data;

namespace OlliBot.Bot.Modules.HumbleBundle;

[DisallowConcurrentExecution]
internal class HumbleBundleUpdateJob(
    ISender sender,
    ILogger<HumbleBundleUpdateJob> logger,
    IDiscordClient discordClient) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken ct = context.CancellationToken;

        logger.LogInformation("Scheduled Humble Bundle update started.");

        foreach (Domain.Enums.HumbleBundleType bundleType in Enum.GetValues<Domain.Enums.HumbleBundleType>())
        {
            try
            {
                await ProcessBundleTypeAsync(bundleType, ct);
            }
            catch (OperationCanceledException)
                when (ct.IsCancellationRequested)
            {
                logger.LogInformation(
                    "Scheduled Humble Bundle update was cancelled.");

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Scheduled Humble Bundle update failed.");

                throw new JobExecutionException(ex);
            }
        }
    }

    private async Task ProcessBundleTypeAsync(
        HumbleBundleType bundleType,
        CancellationToken ct)
    {
        var command = new CheckForHumbleBundleUpdatesCommand(bundleType);

        CheckForHumbleBundleUpdatesResult result =
            await sender.Send(command, ct);

        if (!result.Success)
        {
            logger.LogError(
                "Humble Bundle scan failed for {BundleType}: {Message}",
                bundleType,
                result.Message);

            return;
        }

        if (!result.ScannedBundles.Any())
        {
            logger.LogInformation(
                "No new Humble Bundles found for {BundleType}.",
                bundleType);

            return;
        }

        var userSubscribers = result.Subscribers
            .Where(x => x.SubscriberType == HumbleBundleSubscriberType.User)
            .ToArray();

        var channelSubscribers = result.Subscribers
            .Where(x => x.SubscriberType == HumbleBundleSubscriberType.Channel)
            .ToArray();

        foreach (ScannedHumbleBundle bundle in result.ScannedBundles)
        {
            await NotifyUsersAsync(bundle, userSubscribers, ct);
            await NotifyChannelsAsync(bundle, channelSubscribers, ct);
        }
    }

    private async Task NotifyUsersAsync(
        ScannedHumbleBundle bundle,
        IReadOnlyCollection<HumbleBundleSubscriber> subscribers,
        CancellationToken ct)
    {
        var component = HumbleBundleEmbedBuilder.CreateHumbleBundleComponentV2(bundle);

        foreach (HumbleBundleSubscriber subscriber in subscribers)
        {
            try
            {
                IUser user = await discordClient.GetUserAsync(
                    subscriber.DiscordId,
                    CacheMode.AllowDownload,
                    new RequestOptions
                    {
                        CancelToken = ct
                    });

                if (user is null)
                {
                    logger.LogWarning(
                        "Discord user {UserId} could not be found.",
                        subscriber.DiscordId);

                    continue;
                }

                await user.SendMessageAsync(
                    components: component,
                    options: new RequestOptions
                    {
                        CancelToken = ct
                    });
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to notify Discord user {UserId}.",
                    subscriber.DiscordId);
            }
        }
    }

    private async Task NotifyChannelsAsync(
        ScannedHumbleBundle bundle,
        IReadOnlyCollection<HumbleBundleSubscriber> subscribers,
        CancellationToken ct)
    {
        foreach (HumbleBundleSubscriber subscriber in subscribers)
        {
            try
            {
                if (subscriber.GuildId is not ulong guildId)
                {
                    logger.LogWarning(
                        "Channel subscriber {SubscriberId} has no guild ID.",
                        subscriber.Id);

                    continue;
                }

                IGuild guild = await discordClient.GetGuildAsync(
                    guildId,
                    CacheMode.AllowDownload,
                    new RequestOptions
                    {
                        CancelToken = ct
                    });

                IChannel channel = await guild.GetChannelAsync(
                    subscriber.DiscordId,
                    CacheMode.AllowDownload,
                    new RequestOptions
                    {
                        CancelToken = ct
                    });

                if (channel is not IMessageChannel messageChannel)
                {
                    logger.LogWarning(
                        "Discord channel {ChannelId} was not found or cannot receive messages.",
                        subscriber.DiscordId);

                    continue;
                }

                string? roleMention = null;

                if (subscriber.RoleId is ulong roleId)
                {
                    IRole? role = guild.GetRole(roleId);

                    if (role is null)
                    {
                        logger.LogWarning(
                            "Role {RoleId} no longer exists in guild {GuildId}.",
                            roleId,
                            guildId);
                    }
                    else
                    {
                        roleMention = role.Mention;
                    }
                }
                var component = HumbleBundleEmbedBuilder.CreateHumbleBundleComponentV2(bundle, roleMention);

                await messageChannel.SendMessageAsync(
                    components: component,
                    options: new RequestOptions
                    {
                        CancelToken = ct
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to notify Discord channel {ChannelId}.",
                    subscriber.DiscordId);
            }
        }
    }
    //private async Task ProcessBundleTypeAsync(Domain.Enums.HumbleBundleType bundleType, CancellationToken ct)
    //{
    //    var command = new CheckForHumbleBundleUpdatesCommand(bundleType);

    //    CheckForHumbleBundleUpdatesResult result = await sender.Send(command, ct);
    //    if (!result.Success)
    //    {
    //        logger.LogError(
    //            "Scheduled Humble Bundle update failed: {Message}",
    //            result.Message);
    //    }

    //    if (!result.ScannedBundles.Any())
    //    {
    //        logger.LogInformation("Scheduled Humble Bundle update found no new bundles of type {type}", bundleType);
    //        return false;
    //    }

    //    IEnumerable<Domain.Entities.HumbleBundleSubscriber> userSubscribers = result.Subscribers.Where(s => s.SubscriberType == Domain.Enums.HumbleBundleSubscriberType.User);
    //    IEnumerable<Domain.Entities.HumbleBundleSubscriber> channelSubscribers = result.Subscribers.Where(s => s.SubscriberType == Domain.Enums.HumbleBundleSubscriberType.Channel);

    //    foreach (Application.HumbleBundle.Models.ScannedHumbleBundle bundle in result.ScannedBundles)
    //    {
    //        var userComponent = HumbleBundleEmbedBuilder.CreateHumbleBundleComponentV2(bundle);

    //        foreach (Domain.Entities.HumbleBundleSubscriber? subscriber in userSubscribers)
    //        {
    //            IUser discordUser = await discordClient.GetUserAsync(subscriber.DiscordId);
    //            await discordUser.SendMessageAsync(components: userComponent);
    //        }

    //        foreach (Domain.Entities.HumbleBundleSubscriber? subscriber in channelSubscribers)
    //        {
    //            ulong roleId = subscriber.RoleId ?? 0;

    //            if (subscriber.GuildId is null)
    //            {
    //                logger.LogWarning(
    //                    "Discord channel subscriber {SubscriberId} has no guild ID.",
    //                    subscriber.Id);
    //                continue;
    //            }

    //            IGuild guild = await discordClient.GetGuildAsync(subscriber.GuildId.Value, CacheMode.AllowDownload, new RequestOptions
    //            {
    //                CancelToken = ct
    //            });

    //            IChannel? channel = await guild.GetChannelAsync(
    //                subscriber.DiscordId,
    //                CacheMode.AllowDownload,
    //                new RequestOptions
    //                {
    //                    CancelToken = ct
    //                });

    //            string roleMention;
    //            if (subscriber.RoleId.HasValue && subscriber.RoleId.Value != roleId)
    //            {
    //                IRole role = guild.GetRole(subscriber.RoleId.Value);
    //                roleMention = role.Mention;

    //                // save this for later, maybe worth considering in future

    //                //if (role is not null)
    //                //{
    //                //    embed.Description += $"\n\n<@&{role.Id}>"; // Mention the role in the embed description
    //                //}
    //            }
    //            else if (!subscriber.RoleId.HasValue)
    //            {

    //            }

    //            if (channel is not IMessageChannel messageChannel)
    //            {
    //                logger.LogWarning(
    //                    "Discord channel {ChannelId} was not found or cannot receive messages.",
    //                    subscriber.DiscordId);

    //                return true;
    //            }

    //            await messageChannel.SendMessageAsync(components: userComponent, text: roleMention);
    //        }
    //    }

    //    return null;
    //}
}
