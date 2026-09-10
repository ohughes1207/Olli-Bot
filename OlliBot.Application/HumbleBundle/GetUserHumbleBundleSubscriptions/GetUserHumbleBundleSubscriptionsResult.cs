using OlliBot.Domain.Enums;

namespace OlliBot.Application.HumbleBundle.GetUserHumbleBundleSubscriptions;
public record GetUserHumbleBundleSubscriptionsResult(IReadOnlyCollection<HumbleBundleType> HumbleBundleTypes, ulong? RoleId,bool Success);