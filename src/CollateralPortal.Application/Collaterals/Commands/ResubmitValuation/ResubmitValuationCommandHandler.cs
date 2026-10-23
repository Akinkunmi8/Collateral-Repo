using CollateralPortal.Application.Common.Exceptions;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Domain.Entities;
using CollateralPortal.Domain.Enums;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.ResubmitValuation;

public sealed class ResubmitValuationCommandHandler : IRequestHandler<ResubmitValuationCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValuationPortalClient _valuationClient;

    public ResubmitValuationCommandHandler(IApplicationDbContext db,IValuationPortalClient valuationClient)
    {
        _db = db;
        _valuationClient = valuationClient;
    }

    public async Task Handle(ResubmitValuationCommand request,CancellationToken ct)
    {
        var collateral = await _db.Collaterals.FindAsync(
            new object?[] { request.CollateralId },
            ct)
            ?? throw new NotFoundException(
                nameof(Collateral),
                request.CollateralId);

        /*
         * ---------------------------------------------------------
         * STEP 1
         * Validate workflow state.
         * ---------------------------------------------------------
         */

        if (collateral.CurrentStage != WorkflowStage.Valuation ||
            collateral.Status !=
                CollateralStatus.AdditionalValuationInformationRequested)
        {
            throw new InvalidOperationException(
                "Only collateral awaiting additional valuation information " +
                "can be resubmitted.");
        }

        /*
         * ---------------------------------------------------------
         * STEP 2
         * Confirm that a completed valuation report is available.
         *
         * AccountNumber comes from our trusted collateral record.
         * Never accept the account number from React.
         * ---------------------------------------------------------
         */

        var report = await _valuationClient
            .GetValuationReportAsync(
                collateral.AccountNumber,
                ct);

        if (report is null)
        {
            throw new InvalidOperationException(
                $"No completed valuation report was found for account " +
                $"{collateral.AccountNumber}.");
        }

        /*
         * ---------------------------------------------------------
         * STEP 3
         * Return collateral to active valuation review.
         *
         * IMPORTANT:
         * Do NOT create CollateralValuation here.
         * ---------------------------------------------------------
         */

        collateral.ResubmitForValuationVerification();

        /*
         * ---------------------------------------------------------
         * STEP 4
         * Audit
         * ---------------------------------------------------------
         */

        _db.AuditLogs.Add(new AuditLog
        {
            CollateralId = collateral.Id,
            Action = "ValuationResubmitted",
            PerformedBy = "system", // replace with authenticated user later
            PerformedAtUtc = DateTime.UtcNow,
            Details =
                $"Valuation resubmitted for verification. " +
                $"Response: {request.Response.Trim()}"
        });

        await _db.SaveChangesAsync(ct);
    }
}