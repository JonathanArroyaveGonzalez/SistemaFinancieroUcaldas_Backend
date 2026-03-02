using SAPFIAI.Application.Common.Interfaces;

namespace SAPFIAI.Application.Roles.Queries.GetUserRoles;

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<string>>
{
    private readonly IIdentityService _identityService;

    public GetUserRolesQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<List<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _identityService.GetUserRolesAsync(request.UserId);
        return roles.ToList();
    }
}
