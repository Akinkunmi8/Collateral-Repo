using CollateralPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CollateralPortal.Infrastructure.Persistence.Configurations;
public class CollateralDocumentConfiguration : IEntityTypeConfiguration<CollateralDocument>
{
    public void Configure(EntityTypeBuilder<CollateralDocument> b)
    {
        b.ToTable("CollateralDocuments"); b.HasKey(x=>x.Id); b.Property(x=>x.FileName).HasMaxLength(255).IsRequired(); b.Property(x=>x.ContentType).HasMaxLength(150); b.Property(x=>x.UploadedBy).HasMaxLength(256);
        b.HasOne(x=>x.Collateral).WithMany(x=>x.Documents).HasForeignKey(x=>x.CollateralId).OnDelete(DeleteBehavior.Cascade);
    }
}
