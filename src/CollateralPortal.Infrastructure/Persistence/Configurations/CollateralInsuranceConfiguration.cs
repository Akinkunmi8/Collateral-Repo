using CollateralPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollateralPortal.Infrastructure.Persistence.Configurations;

public class CollateralInsuranceConfiguration : IEntityTypeConfiguration<CollateralInsurance>
{
    public void Configure(EntityTypeBuilder<CollateralInsurance> builder)
    {
        builder.ToTable("CollateralInsurances");
        builder.Property(i => i.CoverageAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.PremiumAmount).HasColumnType("decimal(18,2)");
        builder.Ignore(i => i.DomainEvents);
    }
}
