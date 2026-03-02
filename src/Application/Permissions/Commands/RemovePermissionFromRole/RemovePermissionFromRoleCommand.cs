using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Commands.RemovePermissionFromRole;

public record RemovePermissionFromRoleCommand : IRequest<Result>
{
    public required string RoleId { get; init; }
    public required int PermissionId { get; init; }
}
