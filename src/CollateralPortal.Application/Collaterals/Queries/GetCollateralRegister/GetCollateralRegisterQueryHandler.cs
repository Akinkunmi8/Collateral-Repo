using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralRegister;

public sealed class GetCollateralRegisterQueryHandler
    : IRequestHandler<
        GetCollateralRegisterQuery,
        PagedResult<CollateralRegisterItemVm>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITatService _tatService;

    public GetCollateralRegisterQueryHandler(
        IApplicationDbContext db,
        ITatService tatService)
    {
        _db = db;
        _tatService = tatService;
    }

    public async Task<PagedResult<CollateralRegisterItemVm>> Handle(GetCollateralRegisterQuery request,CancellationToken ct)
    {
        var pageNumber = request.PageNumber < 1
            ? 1
            : request.PageNumber;

        var pageSize = request.PageSize < 1
            ? 20
            : Math.Min(request.PageSize, 100);

        var query = _db.Collaterals
            .AsNoTracking()
            .AsQueryable();

        // ----------------------------------
        // Search
        // ----------------------------------

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(c =>
                c.LoanContractId.Contains(search) ||
                c.CustomerName.Contains(search) ||
                c.AccountNumber.Contains(search));
        }

        // ----------------------------------
        // Stage filter
        // ----------------------------------

        if (!string.IsNullOrWhiteSpace(request.StageFilter))
        {
            var stage = request.StageFilter.Trim();

            if (Enum.TryParse<WorkflowStage>(
                    stage,
                    true,
                    out var parsedStage))
            {
                query = query.Where(c =>
                    c.CurrentStage == parsedStage);
            }
        }

        // ----------------------------------
        // Type filter
        // ----------------------------------

        if (!string.IsNullOrWhiteSpace(request.TypeFilter))
        {
            var type = request.TypeFilter.Trim();

            if (Enum.TryParse<CollateralType>(
                    type,
                    true,
                    out var parsedType))
            {
                query = query.Where(c =>
                    c.Type == parsedType);
            }
        }

        // ----------------------------------
        // Status filter
        // ----------------------------------

        if (!string.IsNullOrWhiteSpace(request.StatusFilter))
        {
            var status = request.StatusFilter.Trim();

            if (Enum.TryParse<CollateralStatus>(
                    status,
                    true,
                    out var parsedStatus))
            {
                query = query.Where(c =>
                    c.Status == parsedStatus);
            }
        }

        // ----------------------------------
        // TAT filter
        // ----------------------------------

        if (!string.IsNullOrWhiteSpace(request.TatFilter))
        {
            var tatFilter = request.TatFilter.Trim();

            if (Enum.TryParse<TatStatus>(
                    tatFilter,
                    true,
                    out var parsedTatStatus))
            {
                var now = DateTime.UtcNow;

                var collateralDataDue =
                    now.AddHours(
                        -_tatService.GetAllowedHours(
                            WorkflowStage.CollateralData));

                var valuationDue =
                    now.AddHours(
                        -_tatService.GetAllowedHours(
                            WorkflowStage.Valuation));

                var perfectionDue =
                    now.AddHours(
                        -_tatService.GetAllowedHours(
                            WorkflowStage.Perfection));

                var insuranceDue =
                    now.AddHours(
                        -_tatService.GetAllowedHours(
                            WorkflowStage.Insurance));

                if (parsedTatStatus == TatStatus.WithinTat)
                {
                    query = query.Where(c =>
                        (
                            c.CurrentStage ==
                                WorkflowStage.CollateralData &&
                            c.StageStartedAtUtc >=
                                collateralDataDue
                        )
                        ||
                        (
                            c.CurrentStage ==
                                WorkflowStage.Valuation &&
                            c.StageStartedAtUtc >=
                                valuationDue
                        )
                        ||
                        (
                            c.CurrentStage ==
                                WorkflowStage.Perfection &&
                            c.StageStartedAtUtc >=
                                perfectionDue
                        )
                        ||
                        (
                            c.CurrentStage ==
                                WorkflowStage.Insurance &&
                            c.StageStartedAtUtc >=
                                insuranceDue
                        ));
                }
                else if (
                    parsedTatStatus ==
                    TatStatus.BeyondTat)
                {
                    query = query.Where(c =>
                        (
                            c.CurrentStage ==
                                WorkflowStage.CollateralData &&
                            c.StageStartedAtUtc <
                                collateralDataDue
                        )
                        ||
                        (
                            c.CurrentStage ==
                                WorkflowStage.Valuation &&
                            c.StageStartedAtUtc <
                                valuationDue
                        )
                        ||
                        (
                            c.CurrentStage ==
                                WorkflowStage.Perfection &&
                            c.StageStartedAtUtc <
                                perfectionDue
                        )
                        ||
                        (
                            c.CurrentStage ==
                                WorkflowStage.Insurance &&
                            c.StageStartedAtUtc <
                                insuranceDue
                        ));
                }
            }
        }

        // ----------------------------------
        // Count AFTER all filters
        // but BEFORE pagination
        // ----------------------------------

        var totalCount =
            await query.CountAsync(ct);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)pageSize);

        // ----------------------------------
        // Pagination
        // ----------------------------------

        var rows = await query.OrderBy(c => c.StageStartedAtUtc)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
    .Select(c => new
    {
        c.Id,
        c.LoanContractId,
        c.CustomerName,
        c.Type,
        c.CurrentStage,
        c.Status,
        c.StageStartedAtUtc,
        c.CreatedAtUtc
    })
    .ToListAsync(ct);

        var items = rows
            .Select(c =>
            {
                string? tatStatus = null;
                double? tatElapsedHours = null;
                int? tatAllowedHours = null;
                DateTime? tatDueAtUtc = null;

                /*
                 * TAT only applies to active workflow stages.
                 */
                if (c.CurrentStage is
                    WorkflowStage.CollateralData or
                    WorkflowStage.Valuation or
                    WorkflowStage.Perfection or
                    WorkflowStage.Insurance)
                {
                    tatStatus = _tatService
                        .GetStatus(
                            c.CurrentStage,
                            c.StageStartedAtUtc)
                        .ToString();

                    tatElapsedHours = _tatService
                        .GetElapsedHours(
                            c.StageStartedAtUtc);

                    tatAllowedHours = _tatService
                        .GetAllowedHours(
                            c.CurrentStage);

                    tatDueAtUtc = _tatService
                        .GetDueDateUtc(
                            c.CurrentStage,
                            c.StageStartedAtUtc);
                }

                return new CollateralRegisterItemVm(
                    c.Id,
                    c.LoanContractId,
                    c.CustomerName,
                    c.Type.ToString(),
                    c.CurrentStage.ToString(),
                    c.Status.ToString(),
                    c.StageStartedAtUtc,
                    c.CreatedAtUtc,
                    tatStatus,
                    tatElapsedHours,
                    tatAllowedHours,
                    tatDueAtUtc);
            })
            .ToList();

        return new PagedResult<CollateralRegisterItemVm>(
            Items: items,
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);
    }
}