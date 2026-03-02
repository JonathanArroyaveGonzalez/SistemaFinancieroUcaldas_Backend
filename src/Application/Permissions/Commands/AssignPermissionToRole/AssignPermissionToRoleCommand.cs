using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Commands.AssignPermissionToRole;

public record AssignPermissionToRoleCommand : IRequest<Result>
{
    public required string RoleId { get; init; }
    public required int PermissionId { get; init; }
    public string? AssignedBy { get; init; }
}
