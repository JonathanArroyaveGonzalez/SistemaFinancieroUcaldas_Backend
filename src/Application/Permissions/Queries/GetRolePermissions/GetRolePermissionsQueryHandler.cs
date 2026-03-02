using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Queries.GetRolePermissions;

public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, List<PermissionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRolePermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionDto>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == request.RoleId)
            .Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description,
                Module = rp.Permission.Module,
                IsActive = rp.Permission.IsActive,
                CreatedAt = rp.Permission.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
