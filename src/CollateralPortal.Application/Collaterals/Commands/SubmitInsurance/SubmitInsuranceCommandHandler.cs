using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Notifications;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using CollateralPortal.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitInsurance;

public sealed class SubmitInsuranceCommandHandler
    : IRequestHandler<SubmitInsuranceCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly IMessagePublisher _messagePublisher;

    public SubmitInsuranceCommandHandler(
        IApplicationDbContext db,
        IMessagePublisher messagePublisher)
    {
        _db = db;
        _messagePublisher = messagePublisher;
    }

    public async Task<Unit> Handle(
        SubmitInsuranceCommand request,
        CancellationToken ct)
    {
        // ---------------------------------------------------------
        // 1. Load collateral and any existing Insurance record
        // ---------------------------------------------------------

        var collateral = await _db.Collaterals
            .Include(x => x.Insurance)
            .FirstOrDefaultAsync(
                x => x.Id == request.CollateralId,
                ct)
            ?? throw new NotFoundException(
                nameof(Collateral),
                request.CollateralId);

        // ---------------------------------------------------------
        // 2. Validate workflow state
        // ---------------------------------------------------------

        if (collateral.CurrentStage != WorkflowStage.Insurance ||
            collateral.Status != CollateralStatus.InInsurance)
        {
            throw new DomainException(
                "Collateral is not currently available for Insurance processing.");
        }

        // ---------------------------------------------------------
        // 3. Insurance report/certificate is mandatory
        // ---------------------------------------------------------

        var insuranceReportExists =
            await _db.CollateralDocuments
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.CollateralId == collateral.Id &&
                        x.DocumentType ==
                            CollateralDocumentType.InsuranceReport,
                    ct);

        if (!insuranceReportExists)
        {
            throw new DomainException(
                "An Insurance report or certificate must be uploaded before completing Insurance.");
        }

        // ---------------------------------------------------------
        // 4. Calculate Insurance policy expiry
        //
        // BRD:
        // ThreeYearly -> 3 years
        // Annual      -> 1 year
        // ---------------------------------------------------------

        var policyStartDate = request.PolicyStartDate.Date;

        var policyExpiryDate =
            request.RenewalCadence ==
            InsuranceRenewalCadence.ThreeYearly
                ? policyStartDate.AddYears(3)
                : policyStartDate.AddYears(1);

        // ---------------------------------------------------------
        // 5. Create or update Insurance record
        // ---------------------------------------------------------

        if (collateral.Insurance is null)
        {
            var insurance = new CollateralInsurance
            {
                CollateralId = collateral.Id,

                PolicyNumber =
                    request.PolicyNumber.Trim(),

                Insurer =
                    request.Insurer.Trim(),

                CoverageAmount =
                    request.CoverageAmount,

                PremiumAmount =
                    request.PremiumAmount,

                PolicyStartDate =
                    policyStartDate,

                PolicyExpiryDate =
                    policyExpiryDate,

                RenewalCadence =
                    request.RenewalCadence,

                RenewalReminderSent =
                    false,

                OverdueNoticeSent =
                    false
            };

            /*
             * Explicitly register this as a new entity.
             * This follows the same pattern used for
             * Legal Perfection.
             */
            _db.CollateralInsurances.Add(insurance);

            collateral.Insurance = insurance;
        }
        else
        {
            /*
             * Existing Insurance record is already tracked
             * because it was loaded using Include().
             */

            collateral.Insurance.PolicyNumber =
                request.PolicyNumber.Trim();

            collateral.Insurance.Insurer =
                request.Insurer.Trim();

            collateral.Insurance.CoverageAmount =
                request.CoverageAmount;

            collateral.Insurance.PremiumAmount =
                request.PremiumAmount;

            collateral.Insurance.PolicyStartDate =
                policyStartDate;

            collateral.Insurance.PolicyExpiryDate =
                policyExpiryDate;

            collateral.Insurance.RenewalCadence =
                request.RenewalCadence;

            /*
             * New/updated policy means renewal monitoring
             * starts afresh.
             */
            collateral.Insurance.RenewalReminderSent =
                false;

            collateral.Insurance.OverdueNoticeSent =
                false;
        }

        // ---------------------------------------------------------
        // 6. Complete Insurance workflow
        //
        // Insurance does NOT send the collateral backwards.
        //
        // Insurance completed
        //       ↓
        // SystemHold / OnHold
        // ---------------------------------------------------------

        collateral.PlaceOnHold();

        _db.AuditLogs.Add(
            new AuditLog
            {
                CollateralId =
                    collateral.Id,

                Action =
                    "InsuranceCompleted",

                PerformedBy = request.Insurer
                    ,
                PerformedAtUtc =
                    DateTime.UtcNow,

                Details =
                    $"Insurance processing completed. " +
                    $"Insurer: {request.Insurer}. " +
                    $"Policy number: {request.PolicyNumber}. " +
                    $"Policy expiry: {request.PolicyStartDate:dd MMM yyyy}."
            });
        // ---------------------------------------------------------
        // 7. Persist database changes FIRST
        // ---------------------------------------------------------

        await _db.SaveChangesAsync(ct);

        // ---------------------------------------------------------
        // 8. Publish notification only after successful commit
        // ---------------------------------------------------------

        var email =
            EmailTemplates.StatusChange(
                collateral.CustomerName,
                collateral.AccountNumber,
                "On Hold",
                $"Insurance has been completed for " +
                $"{collateral.LoanContractId}. " +
                $"Policy {request.PolicyNumber.Trim()} is in place " +
                $"and expires on {policyExpiryDate:dd MMM yyyy}. " +
                "The collateral has been placed on System Hold.");

        await _messagePublisher.PublishAsync(
            "collateral-notifications",
            new CollateralNotificationMessage(
                collateral.Id,
                "Insurance",
                email.Subject,
                email.Body,
                CollateralNotificationMessage.StandardRecipients),
            ct);

        return Unit.Value;
    }
}