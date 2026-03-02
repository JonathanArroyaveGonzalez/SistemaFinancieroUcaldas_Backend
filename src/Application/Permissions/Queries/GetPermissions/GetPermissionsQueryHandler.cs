using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Queries.GetPermissions;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Permissions.AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Module = p.Module,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
