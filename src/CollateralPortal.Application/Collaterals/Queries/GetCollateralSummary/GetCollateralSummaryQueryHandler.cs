using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralSummary;

public sealed class GetCollateralSummaryQueryHandler
    : IRequestHandler<GetCollateralSummaryQuery, CollateralSummaryVm>
{
    private readonly IApplicationDbContext _db;
    private readonly ITatService _tatService;

    public GetCollateralSummaryQueryHandler(
        IApplicationDbContext db,
        ITatService tatService)
    {
        _db = db;
        _tatService = tatService;
    }

    public async Task<CollateralSummaryVm> Handle(
        GetCollateralSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var collaterals = await _db.Collaterals
            .AsNoTracking()
            .Select(x => new
            {
                x.Status,
                x.CurrentStage,
                x.StageStartedAtUtc
            })
            .ToListAsync(cancellationToken);

        /*
         * ----------------------------------------------------
         * ENTERPRISE COUNTS
         * ----------------------------------------------------
         */

        var total = collaterals.Count;

        /*
         * Credit / collateral verification queue.
         */
        var pendingVerification = collaterals.Count(x =>
            x.CurrentStage == WorkflowStage.CollateralData &&
            x.Status == CollateralStatus.PendingApproval);

        /*
         * Valuation queue.
         */
        var pendingValuation = collaterals.Count(x =>
            x.CurrentStage == WorkflowStage.Valuation &&
            x.Status == CollateralStatus.InValuation);

        /*
         * Legal perfection queue.
         */
        var legalPerfection = collaterals.Count(x =>
            x.CurrentStage == WorkflowStage.Perfection &&
            x.Status == CollateralStatus.InPerfection);

        /*
         * Insurance queue.
         */
        var insurance = collaterals.Count(x =>
            x.CurrentStage == WorkflowStage.Insurance &&
            x.Status == CollateralStatus.InInsurance);

        /*
         * Collateral has completed Insurance and is waiting
         * for loan repayment confirmation.
         */
        var onHold = collaterals.Count(x =>
            x.CurrentStage == WorkflowStage.SystemHold &&
            x.Status == CollateralStatus.OnHold);

        /*
         * Permanently archived collateral.
         */
        var archived = collaterals.Count(x =>
            x.CurrentStage == WorkflowStage.Archived &&
            x.Status == CollateralStatus.Archived);

        /*
         * Normally zero because MarkReleased() is immediately
         * followed by Archive() in ReleaseCollateralCommand.
         *
         * Retained for reporting/backward compatibility.
         */
        var released = collaterals.Count(x =>
            x.CurrentStage == WorkflowStage.Release &&
            x.Status == CollateralStatus.Released);

        /*
         * ----------------------------------------------------
         * TAT COUNTS
         * ----------------------------------------------------
         */

        var now = DateTime.UtcNow;

        var activeTatRecords = collaterals
            .Where(x => IsTatApplicable(x.CurrentStage))
            .ToList();

        var withinTat = activeTatRecords.Count(x =>
            _tatService.GetStatus(
                x.CurrentStage,
                x.StageStartedAtUtc,
                now) == TatStatus.WithinTat);

        var beyondTat = activeTatRecords.Count(x =>
            _tatService.GetStatus(
                x.CurrentStage,
                x.StageStartedAtUtc,
                now) == TatStatus.BeyondTat);

        return new CollateralSummaryVm(
            Total: total,
            PendingVerification: pendingVerification,
            PendingValuation: pendingValuation,
            LegalPerfection: legalPerfection,
            Insurance: insurance,
            OnHold: onHold,
            Archived: archived,
            Released: released,
            WithinTat: withinTat,
            BeyondTat: beyondTat);
    }

    private static bool IsTatApplicable(
        WorkflowStage stage)
    {
        return stage is
            WorkflowStage.CollateralData or
            WorkflowStage.Valuation or
            WorkflowStage.Perfection or
            WorkflowStage.Insurance;
    }
}