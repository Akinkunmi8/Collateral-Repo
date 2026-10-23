using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetValuationQueue;

public sealed class GetValuationQueueQueryHandler
    : IRequestHandler<
        GetValuationQueueQuery,
        PagedResult<ValuationQueueItemVm>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITatService _tatService;

    public GetValuationQueueQueryHandler(
        IApplicationDbContext db,
        ITatService tatService)
    {
        _db = db;
        _tatService = tatService;
    }

    public async Task<PagedResult<ValuationQueueItemVm>> Handle(
        GetValuationQueueQuery request,
        CancellationToken ct)
    {
        var pageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        var pageSize =
            request.PageSize < 1
                ? 20
                : Math.Min(request.PageSize, 100);

        /*
         * Start with ALL collateral currently in
         * the Valuation workflow stage.
         *
         * Do NOT filter InValuation here because the
         * Additional Information Requested queue uses
         * another status within the same Valuation stage.
         */
        var query = _db.Collaterals
            .AsNoTracking()
            .Where(x =>
                x.CurrentStage == WorkflowStage.Valuation);

        /*
         * Queue/status filter.
         *
         * Default:
         *      Valuation / InValuation
         *
         * Additional Information tab:
         *      Valuation /
         *      AdditionalValuationInformationRequested
         */
        if (string.Equals(
            request.Status,
            nameof(
                CollateralStatus
                    .AdditionalValuationInformationRequested),
            StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x =>
                x.Status ==
                CollateralStatus
                    .AdditionalValuationInformationRequested);
        }
        else
        {
            query = query.Where(x =>
                x.Status ==
                CollateralStatus.InValuation);
        }

        /*
         * Server-side search.
         */
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search =
                request.Search.Trim();

            query = query.Where(x =>
                x.LoanContractId.Contains(search) ||
                x.CustomerName.Contains(search) ||
                x.CustomerId.Contains(search) ||
                x.AccountNumber.Contains(search));
        }

        /*
         * Count AFTER applying stage/status/search filters.
         */
        var totalCount =
            await query.CountAsync(ct);

        /*
         * Oldest work item first.
         *
         * StageStartedAtUtc is reset whenever the
         * collateral transitions within the workflow.
         */
        var rows = await query
            .OrderBy(x => x.StageStartedAtUtc)
            .Skip(
                (pageNumber - 1) *
                pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.LoanContractId,
                x.CustomerId,
                x.CustomerName,
                x.AccountNumber,
                x.Type,
                x.Region,
                x.Description,
                x.ApprovedLimit,
                x.Currency,
                x.FacilityType,
                x.CollateralAddress,
                x.CollateralOwner,
                x.RelationshipManagerName,
                x.BranchManagerName,
                x.CurrentStage,
                x.Status,
                x.StageStartedAtUtc,
                x.CreatedAtUtc
            })
            .ToListAsync(ct);

        var now =
            DateTime.UtcNow;

        var allowedHours =
            _tatService.GetAllowedHours(
                WorkflowStage.Valuation);

        var items = rows
            .Select(x =>
            {
                var elapsedHours =
                    _tatService.GetElapsedHours(
                        x.StageStartedAtUtc,
                        now);

                var tatStatus =
                    _tatService.GetStatus(
                        WorkflowStage.Valuation,
                        x.StageStartedAtUtc,
                        now);

                var dueAt =
                    _tatService.GetDueDateUtc(
                        WorkflowStage.Valuation,
                        x.StageStartedAtUtc);

                return new ValuationQueueItemVm(
                    Id: x.Id,

                    LoanContractId:
                        x.LoanContractId,

                    CustomerId:
                        x.CustomerId,

                    CustomerName:
                        x.CustomerName,

                    AccountNumber:
                        x.AccountNumber,

                    Type:
                        x.Type.ToString(),

                    Region:
                        x.Region.ToString(),

                    Description:
                        x.Description,

                    ApprovedLimit:
                        x.ApprovedLimit,

                    Currency:
                        x.Currency,

                    FacilityType:
                        x.FacilityType,

                    CollateralAddress:
                        x.CollateralAddress,

                    CollateralOwner:
                        x.CollateralOwner,

                    RelationshipManagerName:
                        x.RelationshipManagerName,

                    BranchManagerName:
                        x.BranchManagerName,

                    CurrentStage:
                        x.CurrentStage.ToString(),

                    Status:
                        x.Status.ToString(),

                    StageStartedAtUtc:
                        x.StageStartedAtUtc,

                    CreatedAtUtc:
                        x.CreatedAtUtc,

                    TatStatus:
                        tatStatus.ToString(),

                    TatElapsedHours:
                        Math.Round(
                            elapsedHours,
                            2),

                    TatAllowedHours:
                        allowedHours,

                    TatDueAtUtc:
                        dueAt
                );
            })
            .ToList();

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)pageSize);

        return new PagedResult<ValuationQueueItemVm>(
            Items: items,
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);
    }
}