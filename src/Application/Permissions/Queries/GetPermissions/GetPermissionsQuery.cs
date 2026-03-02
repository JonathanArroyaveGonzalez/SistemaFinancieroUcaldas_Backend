using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Queries.GetPermissions;

public record GetPermissionsQuery : IRequest<List<PermissionDto>>
{
    public bool ActiveOnly { get; init; } = false;
}
