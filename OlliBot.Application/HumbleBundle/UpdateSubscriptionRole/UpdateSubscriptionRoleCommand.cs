using MediatR;
using OlliBot.Domain.Enums;

namespace OlliBot.Application.HumbleBundle.UpdateSubscriptionRole;
public record UpdateSubscriptionRoleCommand(ulong DiscordId, ulong? RoleId) : IRequest<UpdateSubscriptionRoleResult>;