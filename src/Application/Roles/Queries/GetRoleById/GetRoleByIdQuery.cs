using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Roles.Queries.GetRoleById;

public record GetRoleByIdQuery : IRequest<RoleDto?>
{
    public required string RoleId { get; init; }
}
