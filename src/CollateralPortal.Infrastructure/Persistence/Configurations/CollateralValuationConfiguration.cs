using CollateralPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollateralPortal.Infrastructure.Persistence.Configurations;

public class CollateralValuationConfiguration : IEntityTypeConfiguration<CollateralValuation>
{
    public void Configure(EntityTypeBuilder<CollateralValuation> builder)
    {
        builder.ToTable("CollateralValuations");
        builder.Property(v => v.OpenMarketValue).HasColumnType("decimal(18,2)");
        builder.Property(v => v.ForcedSaleValue).HasColumnType("decimal(18,2)");
        builder.Property(v => v.InsuranceValue).HasColumnType("decimal(18,2)");
        builder.Property(v => v.ActualCoverageRatioPercent).HasColumnType("decimal(6,2)");
        builder.Property(v => v.RequiredCoverageRatioPercent).HasColumnType("decimal(6,2)");

        // Base64 report payloads can be large — nvarchar(max) rather than the EF default.
        builder.Property(v => v.ValuationReportBase64).HasColumnType("nvarchar(max)");

        builder.Ignore(v => v.DomainEvents);
    }
}
