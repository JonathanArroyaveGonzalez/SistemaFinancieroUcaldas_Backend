using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Roles.Commands.AssignRoleToUser;

public record AssignRoleToUserCommand : IRequest<Result>
{
    public required string UserId { get; init; }
    public required string RoleName { get; init; }
}
