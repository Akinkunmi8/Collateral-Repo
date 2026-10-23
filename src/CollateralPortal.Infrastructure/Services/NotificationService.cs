using CollateralPortal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CollateralPortal.Infrastructure.Services;

/// <summary>
/// Placeholder notification sender (logs only). The actual send-to-
/// RM/BM/AM/RH/SG-Head happens via CollateralNotificationConsumer once
/// wired to a real email provider (SendGrid/SMTP) — this exists so
/// Application code has something to call directly if needed outside
/// the queue-based flow.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger) => _logger = logger;

    public Task NotifyStageChangeAsync(Guid collateralId, string stage, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("[Notification] Collateral {CollateralId} — {Stage}: {Message}", collateralId, stage, message);
        return Task.CompletedTask;
    }
}
