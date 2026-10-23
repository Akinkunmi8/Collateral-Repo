using CollateralPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollateralPortal.Infrastructure.Persistence.Configurations;

public class CollateralPerfectionConfiguration : IEntityTypeConfiguration<CollateralPerfection>
{
    public void Configure(EntityTypeBuilder<CollateralPerfection> builder)
    {
        builder.ToTable("CollateralPerfections");
        builder.Ignore(p => p.DomainEvents);
    }
}

public class CollateralReleaseConfiguration : IEntityTypeConfiguration<CollateralRelease>
{
    public void Configure(EntityTypeBuilder<CollateralRelease> builder)
    {
        builder.ToTable("CollateralReleases");
        builder.Ignore(r => r.DomainEvents);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.Property(a => a.Action).IsRequired().HasMaxLength(200);
        builder.Property(a => a.PerformedBy).IsRequired().HasMaxLength(100);
        builder.Ignore(a => a.DomainEvents);
    }
}
