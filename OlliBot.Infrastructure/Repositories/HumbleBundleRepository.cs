using Microsoft.EntityFrameworkCore;
using OlliBot.Application.HumbleBundle;
using OlliBot.Domain.Entities;
using OlliBot.Domain.Enums;
using OlliBot.Infrastructure.Data;

namespace OlliBot.Infrastructure.Repositories;
public class HumbleBundleRepository(OlliBotDbContext db) : IHumbleBundleRepository
{
    public async Task DeleteBundlesAsync(IReadOnlyList<Domain.Entities.HumbleBundle> bundles, CancellationToken ct)
    {
        db.HumbleBundles.RemoveRange(bundles);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteStaleChannelSubscribersAsync(IEnumerable<HumbleBundleSubscriber> subscribers, CancellationToken ct)
    {
        db.HumbleBundleSubscribers.RemoveRange(subscribers);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Domain.Entities.HumbleBundle>> GetCurrentBundlesAsync(HumbleBundleType bundleType, CancellationToken ct)
    {
        return await db.HumbleBundles
            .AsNoTracking()
            .Where(hb => hb.BundleType == bundleType)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HumbleBundleSubscriber>> GetSubscribersAsync(HumbleBundleType bundleType, CancellationToken ct)
    {
        return await db.HumbleBundleSubscribers
            .AsNoTracking()
            .Where(sub => sub.SubscriptionType == bundleType)
            .ToListAsync(ct);
    }

    public async Task AddBundlesAsync(IReadOnlyList<Domain.Entities.HumbleBundle> bundles, CancellationToken ct)
    {
        await db.HumbleBundles.AddRangeAsync(bundles, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddSubscriberAsync(HumbleBundleSubscriber subscriber, CancellationToken ct)
    {
        await db.HumbleBundleSubscribers.AddAsync(subscriber, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> SubscriberExistsAsync(ulong discordId, HumbleBundleType subscriptionType, CancellationToken ct)
    {
        return db.HumbleBundleSubscribers
            .AsNoTracking()
            .AnyAsync(sub => sub.DiscordId == discordId && sub.SubscriptionType == subscriptionType, ct);
    }

    public async Task<IReadOnlyList<HumbleBundleSubscriber>> GetSubscriptions(ulong discordId, CancellationToken ct)
    {
        return await db.HumbleBundleSubscribers
            .AsNoTracking()
            .Where(sub => sub.DiscordId == discordId)
            .ToListAsync(ct);
    }

    public async Task<int> RemoveSubscriberAsync(
        ulong discordId,
        HumbleBundleType bundleType,
        HumbleBundleSubscriberType subscriberType,
        CancellationToken ct)
    {
        return await db.HumbleBundleSubscribers
            .Where(sub =>
                sub.DiscordId == discordId &&
                sub.SubscriptionType == bundleType &&
                sub.SubscriberType == subscriberType)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<Domain.Entities.HumbleBundle?> GetLatestBundle(HumbleBundleType bundleType)
    {
        return await db.HumbleBundles
            .AsNoTracking()
            .Where(hb => hb.BundleType == bundleType)
            .OrderByDescending(hb => hb.ExpiryDate)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateSubscriberRole(ulong discordId, ulong? roleId, CancellationToken cancellationToken)
    {
        await db.HumbleBundleSubscribers
            .Where(x => x.DiscordId == discordId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.RoleId, roleId), cancellationToken);
    }
}
