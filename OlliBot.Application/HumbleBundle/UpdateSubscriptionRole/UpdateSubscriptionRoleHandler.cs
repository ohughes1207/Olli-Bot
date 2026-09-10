using MediatR;

namespace OlliBot.Application.HumbleBundle.UpdateSubscriptionRole;

public class UpdateSubscriptionRoleHandler(
    IHumbleBundleRepository humbleBundleRepository)
    : IRequestHandler<UpdateSubscriptionRoleCommand, UpdateSubscriptionRoleResult>
{
    public async Task<UpdateSubscriptionRoleResult> Handle(
        UpdateSubscriptionRoleCommand request,
        CancellationToken cancellationToken)
    {
        await humbleBundleRepository.UpdateSubscriberRole(
            request.DiscordId,
            request.RoleId,
            cancellationToken);

        return new UpdateSubscriptionRoleResult(
            true,
            "Subscription role updated successfully.");
    }
}