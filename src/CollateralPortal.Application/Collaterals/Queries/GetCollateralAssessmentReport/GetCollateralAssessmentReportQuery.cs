using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralAssessmentReport;

public sealed record GetCollateralAssessmentReportQuery(
    Guid CollateralId
) : IRequest<CollateralAssessmentReportVm>;