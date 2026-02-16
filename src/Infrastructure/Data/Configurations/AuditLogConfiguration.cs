using SAPFIAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SAPFIAI.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45)  // IPv6 max length
            .IsRequired(false);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.Details)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(a => a.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.ErrorMessage)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(a => a.ResourceId)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(a => a.ResourceType)
            .HasMaxLength(100)
            .IsRequired(false);

        // Índices para búsquedas comunes
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_AuditLog_UserId");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLog_Timestamp")
            .IsDescending();

        builder.HasIndex(a => a.Action)
            .HasDatabaseName("IX_AuditLog_Action");

        builder.HasIndex(a => new { a.UserId, a.Timestamp })
            .HasDatabaseName("IX_AuditLog_UserId_Timestamp")
            .IsDescending(false, true);
    }
}
