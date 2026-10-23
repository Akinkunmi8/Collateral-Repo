using CollateralPortal.Domain.Common;

namespace CollateralPortal.Domain.Entities;

public class CollateralRelease : BaseAuditableEntity
{
    public Guid CollateralId { get; set; }
    public Collateral? Collateral { get; set; }

    public bool LoanFullyRepaid { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string ReleasedBy { get; set; } = default!;
    public string? Notes { get; set; }
}
