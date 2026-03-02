using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Commands.RemovePermissionFromRole;

public class RemovePermissionFromRoleCommandHandler : IRequestHandler<RemovePermissionFromRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemovePermissionFromRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemovePermissionFromRoleCommand request, CancellationToken cancellationToken)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId, cancellationToken);

        if (rolePermission == null)
        {
            return Result.Failure(new Error("PermissionNotAssigned", "El permiso no está asignado a este rol"));
        }

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
