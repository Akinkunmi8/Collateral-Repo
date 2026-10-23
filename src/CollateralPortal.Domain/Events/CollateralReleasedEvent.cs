using CollateralPortal.Domain.Common;

namespace CollateralPortal.Domain.Events;

public class CollateralReleasedEvent : BaseDomainEvent
{
    public Guid CollateralId { get; }
    public string ReleasedBy { get; }

    public CollateralReleasedEvent(Guid collateralId, string releasedBy)
    {
        CollateralId = collateralId;
        ReleasedBy = releasedBy;
    }
}
