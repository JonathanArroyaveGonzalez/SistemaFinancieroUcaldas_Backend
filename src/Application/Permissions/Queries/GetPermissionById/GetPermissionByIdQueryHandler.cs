using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Queries.GetPermissionById;

public class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, PermissionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPermissionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PermissionDto?> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Permissions
            .Where(p => p.Id == request.PermissionId)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Module = p.Module,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
