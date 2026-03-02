using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Queries.GetRolePermissions;

public record GetRolePermissionsQuery : IRequest<List<PermissionDto>>
{
    public required string RoleId { get; init; }
}
