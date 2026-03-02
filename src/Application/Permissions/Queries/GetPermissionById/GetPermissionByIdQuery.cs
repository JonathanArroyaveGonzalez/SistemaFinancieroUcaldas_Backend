using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Queries.GetPermissionById;

public record GetPermissionByIdQuery : IRequest<PermissionDto?>
{
    public required int PermissionId { get; init; }
}
