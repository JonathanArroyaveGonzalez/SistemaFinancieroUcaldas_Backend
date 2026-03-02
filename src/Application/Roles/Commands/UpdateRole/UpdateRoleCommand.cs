using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Roles.Commands.UpdateRole;

public record UpdateRoleCommand : IRequest<Result>
{
    public required string RoleId { get; init; }
    public required string NewName { get; init; }
}
