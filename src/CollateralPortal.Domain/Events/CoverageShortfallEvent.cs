using CollateralPortal.Domain.Common;

namespace CollateralPortal.Domain.Events;

public class CoverageShortfallEvent : BaseDomainEvent
{
    public Guid CollateralId { get; }

    public CoverageShortfallEvent(Guid collateralId)
    {
        CollateralId = collateralId;
    }
}
