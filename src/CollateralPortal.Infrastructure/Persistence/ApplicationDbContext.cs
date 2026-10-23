using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Collateral> Collaterals => Set<Collateral>();
    public DbSet<CollateralValuation> CollateralValuations => Set<CollateralValuation>();
    public DbSet<CollateralPerfection> CollateralPerfections => Set<CollateralPerfection>();
    public DbSet<CollateralInsurance> CollateralInsurances => Set<CollateralInsurance>();
    public DbSet<CollateralRelease> CollateralReleases => Set<CollateralRelease>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CollateralDocument> CollateralDocuments => Set<CollateralDocument>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tables (AspNetUsers, AspNetRoles, etc.) must be configured first.
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            else if (entry.State == EntityState.Modified)
                entry.Entity.LastModifiedAtUtc = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(ct);
    }
}
