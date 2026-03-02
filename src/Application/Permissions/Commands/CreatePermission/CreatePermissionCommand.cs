using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Commands.CreatePermission;

public record CreatePermissionCommand : IRequest<Result<int>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Module { get; init; }
}
