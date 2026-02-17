using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAPFIAI.Domain.Entities;

namespace SAPFIAI.Infrastructure.Data.Configurations;

public class IpBlackListConfiguration : IEntityTypeConfiguration<IpBlackList>
{
    public void Configure(EntityTypeBuilder<IpBlackList> builder)
    {
        builder.HasKey(ib => ib.Id);

        builder.Property(ib => ib.IpAddress)
            .IsRequired()
            .HasMaxLength(45);

        builder.Property(ib => ib.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ib => ib.BlockedBy)
            .HasMaxLength(450);

        builder.Property(ib => ib.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(ib => ib.IpAddress);

        builder.HasIndex(ib => ib.BlockedDate);

        builder.Ignore(ib => ib.IsActive);
    }
}
