using SAPFIAI.Domain.Entities;

namespace SAPFIAI.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Permission> Permissions { get; }

    DbSet<RolePermission> RolePermissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
