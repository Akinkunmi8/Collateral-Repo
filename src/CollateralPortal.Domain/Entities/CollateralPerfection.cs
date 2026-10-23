using CollateralPortal.Domain.Common;
using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Domain.Entities;

public class CollateralPerfection : BaseAuditableEntity
{
    public Guid CollateralId { get; set; }
    public Collateral? Collateral { get; set; }

    public string RegistrationReference { get; set; } = default!;
    public string LegalUnit { get; set; } = default!;
    public PerfectionStage Stage { get; set; } = PerfectionStage.InProgress;
    public string? QueryNotes { get; set; }

    // UC-003 — the Legal Team's check that the pledged property/collateral record exists and is valid.
    public bool CollateralValid { get; set; } = true;
    public string? InvalidReason { get; set; }
}
