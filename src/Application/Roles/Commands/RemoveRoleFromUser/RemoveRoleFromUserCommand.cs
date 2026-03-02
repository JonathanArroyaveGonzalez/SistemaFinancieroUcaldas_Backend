using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Roles.Commands.RemoveRoleFromUser;

public record RemoveRoleFromUserCommand : IRequest<Result>
{
    public required string UserId { get; init; }
    public required string RoleName { get; init; }
}
