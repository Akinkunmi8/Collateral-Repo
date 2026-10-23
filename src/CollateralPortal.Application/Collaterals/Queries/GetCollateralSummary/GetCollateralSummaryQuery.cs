using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralSummary;

public sealed record GetCollateralSummaryQuery
    : IRequest<CollateralSummaryVm>;