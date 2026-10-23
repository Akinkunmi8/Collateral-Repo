using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Notifications;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using CollateralPortal.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitPerfection;

public sealed class SubmitPerfectionCommandHandler
    : IRequestHandler<SubmitPerfectionCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly IMessagePublisher _messagePublisher;

    public SubmitPerfectionCommandHandler(
        IApplicationDbContext db,
        IMessagePublisher messagePublisher)
    {
        _db = db;
        _messagePublisher = messagePublisher;
    }

    public async Task<Unit> Handle(
        SubmitPerfectionCommand request,
        CancellationToken ct)
    {
        // -------------------------------------------------
        // 1. Load collateral + existing perfection
        // -------------------------------------------------

        var collateral = await _db.Collaterals
            .Include(x => x.Perfection)
            .FirstOrDefaultAsync(
                x => x.Id == request.CollateralId,
                ct)
            ?? throw new NotFoundException(
                nameof(Collateral),
                request.CollateralId);

        // -------------------------------------------------
        // 2. Validate workflow state
        // -------------------------------------------------

        if (collateral.CurrentStage != WorkflowStage.Perfection ||
            collateral.Status != CollateralStatus.InPerfection)
        {
            throw new DomainException(
                "Collateral is not currently available for Legal Perfection.");
        }

        // -------------------------------------------------
        // 3. Verify mandatory Legal Report
        // -------------------------------------------------

        var legalReportExists =
            await _db.CollateralDocuments
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.CollateralId == collateral.Id &&
                        x.DocumentType ==
                            CollateralDocumentType.LegalReport,
                    ct);

        if (!legalReportExists)
        {
            throw new DomainException(
                "A Legal Perfection report must be uploaded before completing Legal Perfection.");
        }

        // -------------------------------------------------
        // 4. Create/update perfection
        // -------------------------------------------------

        if (collateral.Perfection is null)
        {
            var perfection = new CollateralPerfection
            {
                CollateralId = collateral.Id,
                RegistrationReference = request.RegistrationReference.Trim(),
                LegalUnit = request.LegalUnit.Trim(),
                CollateralValid = request.CollateralValid,
                InvalidReason = request.CollateralValid
                    ? null
                    : request.InvalidReason?.Trim(),
                Stage = PerfectionStage.Completed,
                QueryNotes = null
            };

            _db.CollateralPerfections.Add(perfection);

            collateral.Perfection = perfection;
        }
        else
        {
            collateral.Perfection.RegistrationReference =
                request.RegistrationReference.Trim();

            collateral.Perfection.LegalUnit =
                request.LegalUnit.Trim();

            collateral.Perfection.CollateralValid =
                request.CollateralValid;

            collateral.Perfection.InvalidReason =
                request.CollateralValid
                    ? null
                    : request.InvalidReason?.Trim();

            collateral.Perfection.Stage =
                PerfectionStage.Completed;

            collateral.Perfection.QueryNotes = null;
        }

        // -------------------------------------------------
        // 5. Move workflow forward
        // -------------------------------------------------

        collateral.MoveToInsurance();

        _db.AuditLogs.Add(
            new AuditLog
            {
                CollateralId =
                    collateral.Id,

                Action =
                    "LegalPerfectionCompleted",

                PerformedBy =
                    request.LegalUnit, // use your actual property
                PerformedAtUtc =
                    DateTime.UtcNow,

                Details =
                    $"Legal perfection completed. " +
                    $"Registration reference: {request.RegistrationReference}. " +
                    $"Legal unit: {request.LegalUnit}."
            });

        // -------------------------------------------------
        // 6. Commit database FIRST
        // -------------------------------------------------

        await _db.SaveChangesAsync(ct);

        // -------------------------------------------------
        // 7. Notifications AFTER successful commit
        // -------------------------------------------------

        if (!request.CollateralValid)
        {
            var invalidEmail =
                EmailTemplates.CollateralNotFoundOrInvalid(
                    collateral.CustomerName,
                    collateral.Description,
                    collateral.CollateralAddress ?? "N/A",
                    collateral.Region.ToString(),
                    collateral.FacilityType ?? "N/A");

            await _messagePublisher.PublishAsync(
                "collateral-notifications",
                new CollateralNotificationMessage(
                    collateral.Id,
                    "Perfection",
                    invalidEmail.Subject,
                    invalidEmail.Body,
                    CollateralNotificationMessage.StandardRecipients),
                ct);
        }

        var updateEmail =
            EmailTemplates.CollateralPerfectionUpdate(
                collateral.CustomerName,
                collateral.CollateralAddress ?? "N/A",
                collateral.Region.ToString(),
                collateral.Description);

        await _messagePublisher.PublishAsync(
            "collateral-notifications",
            new CollateralNotificationMessage(
                collateral.Id,
                "Perfection",
                updateEmail.Subject,
                $"{updateEmail.Body} " +
                $"Legal Perfection completed for " +
                $"{collateral.LoanContractId}. " +
                "Collateral moved to Insurance.",
                CollateralNotificationMessage.StandardRecipients),
            ct);

        return Unit.Value;
    }
}