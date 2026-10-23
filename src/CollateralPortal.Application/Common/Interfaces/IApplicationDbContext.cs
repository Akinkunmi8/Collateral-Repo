using CollateralPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Collateral> Collaterals { get; }
    DbSet<CollateralValuation> CollateralValuations { get; }
    DbSet<CollateralPerfection> CollateralPerfections { get; }
    DbSet<CollateralInsurance> CollateralInsurances { get; }
    DbSet<CollateralRelease> CollateralReleases { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<CollateralDocument> CollateralDocuments { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
