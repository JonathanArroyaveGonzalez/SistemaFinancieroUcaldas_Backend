using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Roles.Commands.DeleteRole;

public record DeleteRoleCommand : IRequest<Result>
{
    public required string RoleId { get; init; }
}
