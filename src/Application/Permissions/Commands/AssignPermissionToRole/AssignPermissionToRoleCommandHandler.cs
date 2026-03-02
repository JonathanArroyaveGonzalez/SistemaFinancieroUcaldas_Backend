using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;
using SAPFIAI.Domain.Entities;

namespace SAPFIAI.Application.Permissions.Commands.AssignPermissionToRole;

public class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IRoleService _roleService;

    public AssignPermissionToRoleCommandHandler(IApplicationDbContext context, IRoleService roleService)
    {
        _context = context;
        _roleService = roleService;
    }

    public async Task<Result> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        var roleExists = await _roleService.RoleExistsAsync(request.RoleId);
        if (!roleExists)
        {
            return Result.Failure(new Error("RoleNotFound", "Rol no encontrado"));
        }

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == request.PermissionId, cancellationToken);

        if (permission == null)
        {
            return Result.Failure(new Error("PermissionNotFound", "Permiso no encontrado"));
        }

        var exists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId, cancellationToken);

        if (exists)
        {
            return Result.Failure(new Error("PermissionAlreadyAssigned", "El permiso ya está asignado a este rol"));
        }

        var rolePermission = new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = request.PermissionId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = request.AssignedBy
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
