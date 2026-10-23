using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Notifications;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using CollateralPortal.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Commands.ReleaseCollateral;

public sealed class ReleaseCollateralCommandHandler: IRequestHandler<ReleaseCollateralCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICoreBankingClient _coreBankingClient;
    private readonly IMessagePublisher _messagePublisher;

    public ReleaseCollateralCommandHandler(
        IApplicationDbContext db,
        ICoreBankingClient coreBankingClient,
        IMessagePublisher messagePublisher)
    {
        _db = db;
        _coreBankingClient = coreBankingClient;
        _messagePublisher = messagePublisher;
    }

    public async Task<Unit> Handle(ReleaseCollateralCommand request,CancellationToken ct)
    {
        var collateral = await _db.Collaterals
            .Include(x => x.Release)
            .FirstOrDefaultAsync(
                x => x.Id == request.CollateralId,
                ct)
            ?? throw new NotFoundException(
                nameof(Collateral),
                request.CollateralId);

        // -------------------------------------------------
        // 1. Validate workflow state
        // -------------------------------------------------

        if (
            collateral.CurrentStage != WorkflowStage.SystemHold ||
            collateral.Status != CollateralStatus.OnHold
        )
        {
            throw new DomainException(
                "Only collateral currently on System Hold can be released.");
        }

        // -------------------------------------------------
        // 2. Prevent duplicate release
        // -------------------------------------------------

        if (collateral.Release is not null)
        {
            throw new DomainException(
                "This collateral has already been released.");
        }

        // -------------------------------------------------
        // 3. Re-check repayment from trusted Core Banking
        // -------------------------------------------------

        var facility =
            await _coreBankingClient.GetFacilityAsync(collateral.LoanContractId,ct);

        if (facility is null)
        {
            throw new DomainException(
                $"Unable to retrieve facility information for loan {collateral.LoanContractId}.");
        }

        if (!facility.FullyRepaid)
        {
            throw new DomainException(
                $"Loan {collateral.LoanContractId} is not fully repaid. " +
                $"Outstanding balance: {facility.OutstandingBalance:N2}. " +
                "Collateral cannot be released.");
        }

        // -------------------------------------------------
        // 4. Prepare trusted release information
        // -------------------------------------------------

        var releasedBy =
            request.ReleasedBy.Trim();

        var notes =
            string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim();

        var releaseDate =
            DateTime.UtcNow;

        // -------------------------------------------------
        // 5. Create release record
        // -------------------------------------------------

        var release =
            new CollateralRelease
            {
                CollateralId =
                    collateral.Id,

                LoanFullyRepaid =
                    true,

                ReleaseDate =
                    releaseDate,

                ReleasedBy =
                    releasedBy,

                Notes =
                    notes
            };

        _db.CollateralReleases.Add(release);

        collateral.Release = release;

        // -------------------------------------------------
        // 6. Domain transition
        //
        // SystemHold / OnHold
        //          ↓
        // Release / Released
        //          ↓
        // Archived / Archived
        // -------------------------------------------------

        collateral.MarkReleased(releasedBy);

        collateral.Archive();

        // -------------------------------------------------
        // 7. Persist BEFORE publishing notification
        // -------------------------------------------------
        _db.AuditLogs.Add(
            new AuditLog
            {
                CollateralId = collateral.Id,
                Action = "CollateralReleased",
                PerformedBy = releasedBy,
                PerformedAtUtc = releaseDate,
                Details =
                    $"Collateral released following confirmation of full repayment " +
                    $"for loan {collateral.LoanContractId}." +
                    (
                        string.IsNullOrWhiteSpace(notes)
                            ? string.Empty
                            : $" Notes: {notes}"
                    )
            });

        _db.AuditLogs.Add(
            new AuditLog
            {
                CollateralId = collateral.Id,
                Action = "CollateralArchived",
                PerformedBy = releasedBy,
                PerformedAtUtc = releaseDate,
                Details =
                    "Collateral archived following successful release."
            });

        await _db.SaveChangesAsync(ct);

        // -------------------------------------------------
        // 8. Notification
        // -------------------------------------------------

        var email =
            EmailTemplates.StatusChange(
                collateral.CustomerName,
                collateral.AccountNumber,
                "Released and Archived",
                BuildNotificationMessage(
                    collateral.LoanContractId,
                    releasedBy,
                    releaseDate,
                    notes));

        await _messagePublisher.PublishAsync(
            "collateral-notifications",
            new CollateralNotificationMessage(
                collateral.Id,
                "Release",
                email.Subject,
                email.Body,
                CollateralNotificationMessage.StandardRecipients),
            ct);

        return Unit.Value;
    }

    private static string BuildNotificationMessage(
        string loanContractId,
        string releasedBy,
        DateTime releaseDate,
        string? notes)
    {
        var message =
            $"Collateral for loan {loanContractId} " +
            $"was released by {releasedBy} on " +
            $"{releaseDate:dd MMM yyyy HH:mm} UTC " +
            "following confirmation of full loan repayment. " +
            "The collateral has been archived.";

        if (!string.IsNullOrWhiteSpace(notes))
        {
            message += $" Notes: {notes}";
        }

        return message;
    }
}