using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Permissions.Commands.UpdatePermission;

public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdatePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == request.PermissionId, cancellationToken);

        if (permission == null)
        {
            return Result.Failure(new Error("PermissionNotFound", "Permiso no encontrado"));
        }

        var exists = await _context.Permissions
            .AnyAsync(p => p.Name == request.Name && p.Id != request.PermissionId, cancellationToken);

        if (exists)
        {
            return Result.Failure(new Error("PermissionExists", "Ya existe un permiso con ese nombre"));
        }

        permission.Name = request.Name;
        permission.Description = request.Description;
        permission.Module = request.Module;
        permission.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
