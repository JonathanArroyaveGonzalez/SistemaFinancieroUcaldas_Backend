namespace SAPFIAI.Application.Roles.Queries.GetUserRoles;

public record GetUserRolesQuery : IRequest<List<string>>
{
    public required string UserId { get; init; }
}
