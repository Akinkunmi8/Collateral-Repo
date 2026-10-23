using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Notifications;
using CollateralPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CollateralPortal.Infrastructure.Messaging;

/// <summary>
/// Daily sweep implementing the two BRD-mandated insurance reminders:
///   - 1 month before PolicyExpiryDate: "Insurance renewal notice" (sent once — RenewalReminderSent).
///   - After PolicyExpiryDate elapses without renewal: "renewal of expired insurance" (sent once — OverdueNoticeSent).
/// Both flags reset to false whenever a new/renewed policy is submitted via
/// SubmitInsuranceCommand, so a renewed policy naturally becomes eligible again.
/// </summary>
public class InsuranceRenewalReminderService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);
    private const int ReminderWindowDays = 30;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<InsuranceRenewalReminderService> _logger;

    public InsuranceRenewalReminderService(
        IServiceScopeFactory scopeFactory, IMessagePublisher messagePublisher, ILogger<InsuranceRenewalReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);

        // Run once on startup, then on every tick.
        do
        {
            try
            {
                await RunSweepAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Insurance renewal reminder sweep failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunSweepAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;
        var windowEnd = now.AddDays(ReminderWindowDays);

        var dueForRenewalNotice = await db.CollateralInsurances
            .Include(i => i.Collateral)
            .Where(i => !i.RenewalReminderSent && i.PolicyExpiryDate > now && i.PolicyExpiryDate <= windowEnd)
            .ToListAsync(ct);

        foreach (var insurance in dueForRenewalNotice)
        {
            if (insurance.Collateral is null) continue;

            var daysLeft = (insurance.PolicyExpiryDate - now).Days;
            var email = EmailTemplates.InsuranceRenewalNotice(insurance.Collateral.CustomerName, daysLeft);

            await _messagePublisher.PublishAsync("collateral-notifications", new CollateralNotificationMessage(
                insurance.CollateralId, "Insurance", email.Subject, email.Body, CollateralNotificationMessage.StandardRecipients), ct);

            insurance.RenewalReminderSent = true;
        }

        var dueForOverdueNotice = await db.CollateralInsurances
            .Include(i => i.Collateral)
            .Where(i => !i.OverdueNoticeSent && i.PolicyExpiryDate <= now)
            .ToListAsync(ct);

        foreach (var insurance in dueForOverdueNotice)
        {
            if (insurance.Collateral is null) continue;

            var email = EmailTemplates.InsuranceRenewalOverdue(insurance.Collateral.CustomerName, insurance.PolicyStartDate, insurance.PolicyExpiryDate);

            await _messagePublisher.PublishAsync("collateral-notifications", new CollateralNotificationMessage(
                insurance.CollateralId, "Insurance", email.Subject, email.Body, CollateralNotificationMessage.StandardRecipients), ct);

            insurance.OverdueNoticeSent = true;
        }

        if (dueForRenewalNotice.Count > 0 || dueForOverdueNotice.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
