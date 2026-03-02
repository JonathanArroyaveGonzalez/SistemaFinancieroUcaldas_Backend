using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Commands.DeletePermission;

public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeletePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == request.PermissionId, cancellationToken);

        if (permission == null)
        {
            return Result.Failure(new Error("PermissionNotFound", "Permiso no encontrado"));
        }

        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.PermissionId == request.PermissionId)
            .ToListAsync(cancellationToken);

        _context.RolePermissions.RemoveRange(rolePermissions);
        _context.Permissions.Remove(permission);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
