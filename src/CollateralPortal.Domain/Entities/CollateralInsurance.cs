using CollateralPortal.Domain.Common;
using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Domain.Entities;

public class CollateralInsurance : BaseAuditableEntity
{
    public Guid CollateralId { get; set; }
    public Collateral? Collateral { get; set; }

    public string PolicyNumber { get; set; } = default!;
    public string Insurer { get; set; } = default!;
    public decimal CoverageAmount { get; set; } // "Sum insured amount" / Limit on Risk Asset
    public decimal PremiumAmount { get; set; }
    public DateTime PolicyStartDate { get; set; }
    public DateTime PolicyExpiryDate { get; set; }
    public InsuranceRenewalCadence RenewalCadence { get; set; }
    public bool RenewalReminderSent { get; set; }
    public bool OverdueNoticeSent { get; set; }
}
