using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Roles.Commands.CreateRole;

public record CreateRoleCommand : IRequest<Result<string>>
{
    public required string Name { get; init; }
}
