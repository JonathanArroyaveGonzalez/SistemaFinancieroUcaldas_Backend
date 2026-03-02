using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Commands.UpdatePermission;

public record UpdatePermissionCommand : IRequest<Result>
{
    public required int PermissionId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Module { get; init; }
    public bool IsActive { get; init; } = true;
}
