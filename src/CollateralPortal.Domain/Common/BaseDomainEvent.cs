namespace CollateralPortal.Domain.Common;

public abstract class BaseDomainEvent
{
    public DateTime OccurredOnUtc { get; protected set; } = DateTime.UtcNow;
}
