namespace CollateralPortal.Application.Common.Interfaces;

/// <summary>
/// Email notification port — sends the RM/BM/AM/RH/SG Head alerts required
/// at every workflow step per the BRD. Actual send is queued via
/// IMessagePublisher / Azure Service Bus rather than sent inline.
/// </summary>
public interface INotificationService
{
    Task NotifyStageChangeAsync(Guid collateralId, string stage, string message, CancellationToken ct = default);
}
