using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Application.Collaterals.Queries.GetArchivedCollaterals;

public sealed class GetArchivedCollateralsQueryHandler
    : IRequestHandler<
        GetArchivedCollateralsQuery,
        PagedResult<ArchivedCollateralVm>>
{
    private readonly IApplicationDbContext _db;

    public GetArchivedCollateralsQueryHandler(
        IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ArchivedCollateralVm>> Handle(
        GetArchivedCollateralsQuery request,
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

        var query = _db.Collaterals
            .AsNoTracking()
            .Where(x =>
                x.CurrentStage == WorkflowStage.Archived &&
                x.Status == CollateralStatus.Archived &&
                x.Release != null);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.LoanContractId.Contains(search) ||
                x.CustomerName.Contains(search) ||
                x.AccountNumber.Contains(search));
        }

        var totalCount =
            await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(x => x.Release!.ReleaseDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ArchivedCollateralVm(
                x.Id,
                x.LoanContractId,
                x.CustomerName,
                x.AccountNumber,
                x.Type.ToString(),
                x.Region.ToString(),
                x.ApprovedLimit,
                x.Currency,
                x.Release!.ReleaseDate,
                x.Release.ReleasedBy,
                x.Release.LoanFullyRepaid,
                x.Release.Notes))
            .ToListAsync(ct);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount / (double)pageSize);

        return new PagedResult<ArchivedCollateralVm>(
            rows,
            pageNumber,
            pageSize,
            totalCount,
            totalPages);
    }
}