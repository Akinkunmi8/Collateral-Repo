using CollateralPortal.Application.Common.Models;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetArchivedCollaterals;

public sealed record GetArchivedCollateralsQuery(
    string? Search,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<ArchivedCollateralVm>>;