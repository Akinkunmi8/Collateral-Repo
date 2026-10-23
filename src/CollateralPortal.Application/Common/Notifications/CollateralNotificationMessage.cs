namespace CollateralPortal.Application.Common.Notifications;

/// <summary>
/// Standard envelope every workflow handler publishes to the
/// "collateral-notifications" queue. Recipients are role labels (RM/BM/AM/
/// RH/SG Head per the BRD) — kept as plain strings here since Application
/// cannot reference the Identity roles in Infrastructure.
/// </summary>
public record CollateralNotificationMessage(
    Guid CollateralId,
    string Stage,
    string Subject,
    string Body,
    string[] Recipients)
{
    public static readonly string[] StandardRecipients = { "RM", "BM", "AM", "RH", "SgHead" };
}
