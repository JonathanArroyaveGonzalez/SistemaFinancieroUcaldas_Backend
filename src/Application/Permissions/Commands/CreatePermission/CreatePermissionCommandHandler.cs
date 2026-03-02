using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;
using SAPFIAI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SAPFIAI.Application.Permissions.Commands.CreatePermission;

public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreatePermissionCommandHandler> _logger;

    public CreatePermissionCommandHandler(IApplicationDbContext context, ILogger<CreatePermissionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating permission: {PermissionName}", request.Name);
        
        var exists = await _context.Permissions
            .AnyAsync(p => p.Name == request.Name, cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Permission already exists: {PermissionName}", request.Name);
            return Result.Failure<int>(new Error("PermissionExists", "El permiso ya existe"));
        }

        var permission = new Permission
        {
            Name = request.Name,
            Description = request.Description,
            Module = request.Module,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission created successfully: {PermissionId}", permission.Id);
        return Result.Success(permission.Id);
    }
}
