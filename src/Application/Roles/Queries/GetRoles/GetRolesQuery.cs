using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Roles.Queries.GetRoles;

public record GetRolesQuery : IRequest<List<RoleDto>>;
