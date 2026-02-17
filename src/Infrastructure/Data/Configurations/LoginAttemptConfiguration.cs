using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAPFIAI.Domain.Entities;

namespace SAPFIAI.Infrastructure.Data.Configurations;

public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.HasKey(la => la.Id);

        builder.Property(la => la.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(la => la.IpAddress)
            .IsRequired()
            .HasMaxLength(45);

        builder.Property(la => la.UserAgent)
            .HasMaxLength(500);

        builder.Property(la => la.FailureReason)
            .HasMaxLength(500);

        builder.HasIndex(la => la.Email);

        builder.HasIndex(la => la.IpAddress);

        builder.HasIndex(la => la.AttemptDate);

        builder.HasIndex(la => new { la.Email, la.AttemptDate });

        builder.HasIndex(la => new { la.IpAddress, la.AttemptDate });
    }
}
