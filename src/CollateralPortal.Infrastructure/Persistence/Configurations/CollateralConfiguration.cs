using CollateralPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollateralPortal.Infrastructure.Persistence.Configurations;

public class CollateralConfiguration : IEntityTypeConfiguration<Collateral>
{
    public void Configure(EntityTypeBuilder<Collateral> builder)
    {
        builder.ToTable("CollateralRegister");
        builder.Property(c => c.LoanContractId).IsRequired().HasMaxLength(50);
        builder.Property(c => c.AccountNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.CustomerId).IsRequired().HasMaxLength(50);
        builder.Property(c => c.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(500);
        builder.Property(c => c.ApprovedLimit).HasColumnType("decimal(18,2)");
        builder.Property(c => c.RequiredCoverageRatioPercent).HasColumnType("decimal(5,2)");

        builder.HasIndex(c => c.LoanContractId);

        builder.HasOne(c => c.Valuation).WithOne(v => v.Collateral!)
            .HasForeignKey<CollateralValuation>(v => v.CollateralId);
        builder.HasOne(c => c.Perfection).WithOne(p => p.Collateral!)
            .HasForeignKey<CollateralPerfection>(p => p.CollateralId);
        builder.HasOne(c => c.Insurance).WithOne(i => i.Collateral!)
            .HasForeignKey<CollateralInsurance>(i => i.CollateralId);
        builder.HasOne(c => c.Release).WithOne(r => r.Collateral!)
            .HasForeignKey<CollateralRelease>(r => r.CollateralId);

        builder.HasMany(c => c.AuditLogs).WithOne(a => a.Collateral!)
            .HasForeignKey(a => a.CollateralId);

        builder.Ignore(c => c.DomainEvents);
    }
}
