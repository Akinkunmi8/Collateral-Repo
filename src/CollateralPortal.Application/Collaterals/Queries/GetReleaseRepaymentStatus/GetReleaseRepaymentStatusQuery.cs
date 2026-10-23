using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetReleaseRepaymentStatus;

public sealed record GetReleaseRepaymentStatusQuery(
    Guid CollateralId
) : IRequest<ReleaseRepaymentStatusVm>;

public sealed record ReleaseRepaymentStatusVm(
    Guid CollateralId,
    string LoanContractId,
    decimal OutstandingBalance,
    DateTime NextRepaymentDate,
    bool FullyRepaid,
    string Branch,
    string Region);