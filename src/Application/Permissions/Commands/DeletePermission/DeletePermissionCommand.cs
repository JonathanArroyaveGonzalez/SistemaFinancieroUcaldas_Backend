using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Commands.DeletePermission;

public record DeletePermissionCommand : IRequest<Result>
{
    public required int PermissionId { get; init; }
}
