using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Application.Common.Notifications;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitValuation;

public sealed class SubmitValuationCommandHandler
    : IRequestHandler<SubmitValuationCommand, SubmitValuationResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IValuationPortalClient _valuationClient;
    private readonly IMessagePublisher _messagePublisher;

    public SubmitValuationCommandHandler(
        IApplicationDbContext db,
        IValuationPortalClient valuationClient,
        IMessagePublisher messagePublisher)
    {
        _db = db;
        _valuationClient = valuationClient;
        _messagePublisher = messagePublisher;
    }

    public async Task<SubmitValuationResult> Handle(SubmitValuationCommand request,CancellationToken ct)
    {
        var collateral = await _db.Collaterals.FindAsync(
            new object?[] { request.CollateralId },
            ct)
            ?? throw new NotFoundException(
                nameof(Collateral),
                request.CollateralId);

        // Only a collateral currently in Valuation can be processed here.
        if (collateral.CurrentStage != WorkflowStage.Valuation ||
            collateral.Status != CollateralStatus.InValuation)
        {
            throw new InvalidOperationException(
                "Only collateral currently in Valuation can be processed.");
        }

        decimal? openMarketValue;
        decimal? forcedSaleValue;
        string? valuersName;
        DateTime? valuationDate;

        ValuationReportDto? report = null;

        /*
         * ---------------------------------------------------------
         * STEP 1
         * Obtain the valuation information.
         *
         * IMPORTANT:
         * Nothing is persisted into CollateralValuation here.
         * ---------------------------------------------------------
         */

        if (request.ManualEntry)
        {
            openMarketValue = request.ManualOpenMarketValue;
            forcedSaleValue = request.ManualForcedSaleValue;
            valuersName = request.ManualValuersName;
            valuationDate = request.ManualValuationDate;
        }
        else
        {
            // Account number is obtained from the trusted
            // collateral record, never from the frontend.
            report = await _valuationClient
                .GetValuationReportAsync(
                    collateral.AccountNumber,
                    ct)
                ?? throw new Domain.Exceptions.DomainException(
                    $"No completed valuation report found for account {collateral.AccountNumber}.");

            openMarketValue = report.OpenMarketValue;
            forcedSaleValue = report.ForceSalesValue;
            valuersName = report.ValuersName;
            valuationDate = report.ValuationDate;
        }

        /*
         * ---------------------------------------------------------
         * STEP 2
         * Calculate coverage.
         *
         * Coverage informs the officer's decision.
         * It does not automatically make the decision.
         * ---------------------------------------------------------
         */

        var requiredPercent =
            CoverageRatioRules.RequiredPercent(
                collateral.Type,
                collateral.Region);

        var actualPercent =
            collateral.ApprovedLimit == 0 ||
            forcedSaleValue is null or 0
                ? 0
                : Math.Round(
                    forcedSaleValue.Value /
                    collateral.ApprovedLimit * 100m,
                    2);

        var adequate =
            actualPercent >= requiredPercent;

        /*
         * ---------------------------------------------------------
         * STEP 3
         * REQUEST ADDITIONAL VALUATION INFORMATION
         *
         * Do NOT create CollateralValuation.
         * Only update workflow + audit + notification.
         * ---------------------------------------------------------
         */

        if (request.Decision ==
            ValuationDecision.RequestAdditionalValuationInformation)
        {
            collateral.RequestAdditionalValuationInformation();

            _db.AuditLogs.Add(new AuditLog
            {
                CollateralId = collateral.Id,
                Action = "AdditionalValuationInformationRequested",
                PerformedBy = "system", // replace with authenticated user later
                PerformedAtUtc = DateTime.UtcNow,
                Details =
                    $"Additional valuation information requested. " +
                    $"Remark: {request.Remark.Trim()}"
            });

            await _db.SaveChangesAsync(ct);

            var email =
                EmailTemplates.AdditionalCollateralRequested(
                    collateral.CustomerName,
                    collateral.AccountNumber,
                    actualPercent,
                    requiredPercent,
                    request.Remark);

            await _messagePublisher.PublishAsync(
                "collateral-notifications",
                new CollateralNotificationMessage(
                    collateral.Id,
                    "Valuation",
                    email.Subject,
                    email.Body,
                    CollateralNotificationMessage.StandardRecipients),
                ct);

            return new SubmitValuationResult(
                adequate,
                actualPercent,
                requiredPercent,
                request.Decision);
        }

        /*
         * ---------------------------------------------------------
         * STEP 4
         * APPROVE VALUATION
         *
         * This is the ONLY path that creates CollateralValuation.
         * ---------------------------------------------------------
         */

        if (request.Decision != ValuationDecision.Submit)
        {
            throw new InvalidOperationException(
                "Unsupported valuation decision.");
        }

        var valuation = new CollateralValuation
        {
            CollateralId = collateral.Id,
            ManualEntry = request.ManualEntry,

            OpenMarketValue = openMarketValue,
            ForcedSaleValue = forcedSaleValue,
            ValuersName = valuersName,
            ValuationDate = valuationDate,

            RequiredCoverageRatioPercent = requiredPercent,
            ActualCoverageRatioPercent = actualPercent,
            CoverageAdequate = adequate,

            Decision = request.Decision,
            Remark = request.Remark
        };

        /*
         * ---------------------------------------------------------
         * AUTOMATIC VALUATION
         *
         * Copy the trusted Valuation Portal response into our
         * permanent approved snapshot.
         * ---------------------------------------------------------
         */

        if (!request.ManualEntry)
        {
            if (report is null)
            {
                throw new InvalidOperationException(
                    "The valuation report could not be retrieved.");
            }

            valuation.FacilityRef =
                report.FacilityRef;

            valuation.PropertyDescription =
                report.PropertyDescription;

            valuation.Landsize =
                report.Landsize;

            valuation.State =
                report.State;

            valuation.LocalGov =
                report.LocalGov;

            valuation.InsuranceValue =
                report.InsuranceValue;

            valuation.SalesLocation =
                report.SalesLocation;

            valuation.SalesDate =
                report.SalesDate;

            valuation.ValuationReportName =
                report.ValuationReportName;

            valuation.DateAssigned =
                report.DateAssigned;

            /*
             * IMPORTANT:
             * On the automatic path, the report comes from the
             * trusted Valuation Portal.
             *
             * Do not accept a Base64 report supplied by React
             * as the authoritative report.
             */
            valuation.ValuationReportBase64 =
                report.ValuationReportBase64;

            valuation.ContentType =
                report.ContentType;
        }
        else
        {
            /*
             * Manual path:
             * report must be supplied by the user because there
             * is no external completed valuation report.
             */
            valuation.ValuationReportBase64 =
                request.ReportBase64;

            valuation.ContentType =
                request.ReportContentType;
        }

        /*
         * ---------------------------------------------------------
         * STEP 5
         * Persist approved valuation and move workflow.
         * ---------------------------------------------------------
         */

        _db.CollateralValuations.Add(valuation);

        collateral.Valuation = valuation;

        collateral.MoveToPerfection();

        _db.AuditLogs.Add(new AuditLog
        {
            CollateralId = collateral.Id,
            Action = "ValuationApproved",
            PerformedBy = "system", // authenticated user later
            PerformedAtUtc = DateTime.UtcNow,
            Details =
                $"Valuation approved. " +
                $"Actual coverage ratio: {actualPercent:N2}%. " +
                $"Required coverage ratio: {requiredPercent:N2}%. " +
                $"Remark: {request.Remark.Trim()}"
        });

        await _db.SaveChangesAsync(ct);

        /*
         * ---------------------------------------------------------
         * STEP 6
         * Notification
         * ---------------------------------------------------------
         */

        var approvalEmail =
            EmailTemplates.ValuationReportUploaded(
                collateral.CustomerName,
                collateral.AccountNumber,
                collateral.CollateralOwner,
                collateral.CollateralAddress,
                collateral.ApprovedLimit,
                forcedSaleValue,
                openMarketValue);

        await _messagePublisher.PublishAsync(
            "collateral-notifications",
            new CollateralNotificationMessage(
                collateral.Id,
                "Valuation",
                approvalEmail.Subject,
                approvalEmail.Body,
                CollateralNotificationMessage.StandardRecipients),
            ct);

        return new SubmitValuationResult(
            adequate,
            actualPercent,
            requiredPercent,
            request.Decision);
    }
}