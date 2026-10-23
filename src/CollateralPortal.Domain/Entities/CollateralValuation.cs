using CollateralPortal.Domain.Common;
using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Domain.Entities;

/// <summary>
/// Populated from the real Valuation Portal's CompletedValuationReportDto
/// (fetched by account number), or manually entered when no completed
/// report exists yet for the account.
/// </summary>
public class CollateralValuation : BaseAuditableEntity
{
    public Guid CollateralId { get; set; }
    public Collateral? Collateral { get; set; }

    public bool ManualEntry { get; set; }

    public string? FacilityRef { get; set; }
    public string? PropertyDescription { get; set; }
    public string? Landsize { get; set; }
    public string? State { get; set; }
    public string? LocalGov { get; set; }

    public DateTime? ValuationDate { get; set; }
    public DateTime? ValuationExpiryDate { get; set; }
    public decimal? OpenMarketValue { get; set; }
    public decimal? ForcedSaleValue { get; set; }
    public decimal? InsuranceValue { get; set; }
    public string? SalesLocation { get; set; }
    public DateTime? SalesDate { get; set; }

    public string? ValuationReportName { get; set; }
    public string? ValuersName { get; set; }
    public DateTime? DateAssigned { get; set; }

    public string? ValuationReportBase64 { get; set; }
    public string? ContentType { get; set; }

    public decimal ActualCoverageRatioPercent { get; set; }
    public decimal RequiredCoverageRatioPercent { get; set; }
    public bool CoverageAdequate { get; set; }

    // User-driven decision per the workstep UI — coverage ratio informs it, doesn't dictate it.
    public ValuationDecision Decision { get; set; }
    public string? Remark { get; set; }
}
