using OlliBot.Domain.Entities;
using OlliBot.Domain.Enums;

namespace OlliBot.Application.HumbleBundle;
public interface IHumbleBundleRepository
{
    Task AddBundlesAsync(IReadOnlyList<Domain.Entities.HumbleBundle> bundles, CancellationToken ct);

    Task<IReadOnlyList<Domain.Entities.HumbleBundle>> GetCurrentBundlesAsync(HumbleBundleType bundleType, CancellationToken ct);

    Task<IReadOnlyList<HumbleBundleSubscriber>> GetSubscribersAsync(HumbleBundleType bundleType, CancellationToken ct);

    Task AddSubscriberAsync(HumbleBundleSubscriber subscriber, CancellationToken ct);

    Task DeleteStaleChannelSubscribersAsync(IEnumerable<HumbleBundleSubscriber> subscribers, CancellationToken ct);

    Task DeleteBundlesAsync(IReadOnlyList<Domain.Entities.HumbleBundle> bundles, CancellationToken ct);

    Task<bool> SubscriberExistsAsync(ulong discordId, HumbleBundleType subscriptionType, CancellationToken ct);

    Task<IReadOnlyList<HumbleBundleSubscriber>> GetSubscriptions(ulong discordId, CancellationToken ct);

    Task<int> RemoveSubscriberAsync(ulong discordId, HumbleBundleType humbleBundleType, HumbleBundleSubscriberType subscriberType, CancellationToken cancellationToken);

    Task<Domain.Entities.HumbleBundle?> GetLatestBundle(HumbleBundleType bundleType);
    
    Task UpdateSubscriberRole(ulong discordId, ulong? roleId, CancellationToken cancellationToken);
}
