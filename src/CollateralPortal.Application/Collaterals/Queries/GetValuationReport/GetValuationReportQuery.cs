using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetValuationReport;

public sealed record GetValuationReportQuery(
    Guid CollateralId)
    : IRequest<ValuationReportVm>;