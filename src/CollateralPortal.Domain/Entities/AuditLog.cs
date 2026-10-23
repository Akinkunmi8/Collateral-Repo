using CollateralPortal.Domain.Common;

namespace CollateralPortal.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid CollateralId { get; set; }
    public Collateral? Collateral { get; set; }

    public string Action { get; set; } = default!;
    public string PerformedBy { get; set; } = default!;
    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}
