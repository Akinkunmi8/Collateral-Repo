using CollateralPortal.Domain.Common;
using CollateralPortal.Domain.Enums;

namespace CollateralPortal.Domain.Events;

public class CollateralSubmittedEvent : BaseDomainEvent
{
    public Guid CollateralId { get; }
    public WorkflowStage Stage { get; }

    public CollateralSubmittedEvent(Guid collateralId, WorkflowStage stage)
    {
        CollateralId = collateralId;
        Stage = stage;
    }
}
