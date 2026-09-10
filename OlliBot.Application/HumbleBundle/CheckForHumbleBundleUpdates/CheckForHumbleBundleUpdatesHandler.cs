using MediatR;
using Microsoft.Extensions.Logging;
using OlliBot.Application.HumbleBundle.Models;
using OlliBot.Domain.Entities;

namespace OlliBot.Application.HumbleBundle.CheckForHumbleBundleUpdates;
public class CheckForHumbleBundleUpdatesHandler(
    ILogger<CheckForHumbleBundleUpdatesHandler> logger,
    IHumbleBundleRepository humbleBundleRepository,
    IHumbleBundleScanner humbleBundleScanner,
    IDiscordSubscriberValidater subscriberValidater) : IRequestHandler<CheckForHumbleBundleUpdatesCommand, CheckForHumbleBundleUpdatesResult>
{
    public async Task<CheckForHumbleBundleUpdatesResult> Handle(CheckForHumbleBundleUpdatesCommand command, CancellationToken ct)
    {
        IReadOnlyCollection<ScannedHumbleBundle> scannedBundles = await humbleBundleScanner.ScanAsync(command.BundleType, ct);

        IReadOnlyList<Domain.Entities.HumbleBundle> knownBundles = await humbleBundleRepository.GetCurrentBundlesAsync(command.BundleType, ct);

        var knownUrls = knownBundles
            .Select(bundle => bundle.Url)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        ScannedHumbleBundle[] newBundles = scannedBundles
            .Where(bundle => !knownUrls.Contains(bundle.Url))
            .ToArray();

        await CheckForExpiredBundles(scannedBundles, knownBundles, ct);

        if (newBundles.Length == 0)
        {
            return new CheckForHumbleBundleUpdatesResult(true, "No new bundles found", [], []);
        }

        logger.LogInformation(
            "Humble Bundle scan found {NewBundleCount} new bundles for {BundleType}",
            newBundles.Length,
            command.BundleType);

        await SaveNewBundles(newBundles, ct);

        IReadOnlyList<HumbleBundleSubscriber> subscribers = await humbleBundleRepository.GetSubscribersAsync(command.BundleType, ct);
        subscribers = await RemoveStaleSubscribersAsync(subscribers, ct);

        var result = new CheckForHumbleBundleUpdatesResult(
            true,
            $"Found {newBundles.Length} new bundles for {command.BundleType}",
            newBundles,
            subscribers);

        return result;
    }

    private async Task<IReadOnlyList<HumbleBundleSubscriber>> RemoveStaleSubscribersAsync(
        IReadOnlyList<HumbleBundleSubscriber> subscribers,
        CancellationToken ct)
    {
        IEnumerable<HumbleBundleSubscriber> staleSubscribers = subscriberValidater
            .FindStaleSubscribers(subscribers
            .Where(s => s.SubscriberType == Domain.Enums.HumbleBundleSubscriberType.Channel));

        if (!staleSubscribers.Any())
        {
            return subscribers;
        }

        await humbleBundleRepository.DeleteStaleChannelSubscribersAsync(
            staleSubscribers,
            ct);

        var staleSubscriberIds = staleSubscribers
            .Select(subscriber => subscriber.Id)
            .ToHashSet();

        return subscribers
            .Where(subscriber => !staleSubscriberIds.Contains(subscriber.Id))
            .ToArray();
    }

    private async Task SaveNewBundles(ScannedHumbleBundle[] newBundles, CancellationToken ct)
    {
        Domain.Entities.HumbleBundle[] entities = newBundles
            .Select(MapToEntity)
            .ToArray();

        await humbleBundleRepository.AddBundlesAsync(
            entities,
            ct);
    }

    private async Task CheckForExpiredBundles(IReadOnlyCollection<ScannedHumbleBundle> scannedBundles, IReadOnlyList<Domain.Entities.HumbleBundle> knownBundles, CancellationToken ct)
    {
        var scannedUrls = scannedBundles
            .Select(bundle => bundle.Url)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Domain.Entities.HumbleBundle[] expiredBundles = knownBundles
            .Where(bundle => !scannedUrls.Contains(bundle.Url))
            .ToArray();

        if (expiredBundles.Length > 0)
        {
            logger.LogInformation("{BundleCount} Bundles are expired", expiredBundles.Length);
            await humbleBundleRepository.DeleteBundlesAsync(
                expiredBundles,
                ct);
        }
    }

    private static Domain.Entities.HumbleBundle MapToEntity(
    ScannedHumbleBundle bundle)
    {
        return new Domain.Entities.HumbleBundle
        {
            Name = bundle.Name,
            BundleType = bundle.BundleType,
            ExpiryDate = bundle.ExpiryDate,
            Url = bundle.Url,
            DateSeen = DateTime.UtcNow
        };
    }
}
