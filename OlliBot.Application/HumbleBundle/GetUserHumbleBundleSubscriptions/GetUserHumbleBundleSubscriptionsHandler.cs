using MediatR;
using Microsoft.Extensions.Logging;

namespace OlliBot.Application.HumbleBundle.GetUserHumbleBundleSubscriptions;
public class GetUserHumbleBundleSubscriptionsHandler(
    ILogger<GetUserHumbleBundleSubscriptionsHandler> logger,
    IHumbleBundleRepository humbleBundleRepository) : IRequestHandler<GetUserHumbleBundleSubscriptionsQuery, GetUserHumbleBundleSubscriptionsResult>
{
    public async Task<GetUserHumbleBundleSubscriptionsResult> Handle(GetUserHumbleBundleSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Entities.HumbleBundleSubscriber> humbleBundleTypes = await humbleBundleRepository.GetSubscriptions(request.DiscordId, cancellationToken);

        return new GetUserHumbleBundleSubscriptionsResult(
            humbleBundleTypes.Select(sub => sub.SubscriptionType).ToList(), 
            humbleBundleTypes.Select(sub => sub.RoleId).FirstOrDefault(), 
            true);
    }
}
