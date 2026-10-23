using CollateralPortal.Domain.Common;

namespace CollateralPortal.Domain.Events;

public class InsuranceExpiringEvent : BaseDomainEvent
{
    public Guid CollateralId { get; }
    public DateTime ExpiryDate { get; }

    public InsuranceExpiringEvent(Guid collateralId, DateTime expiryDate)
    {
        CollateralId = collateralId;
        ExpiryDate = expiryDate;
    }
}
