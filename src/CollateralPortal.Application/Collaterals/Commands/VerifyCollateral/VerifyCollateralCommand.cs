using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.VerifyCollateral;

public enum CollateralVerificationDecision
{
    Approve = 1,
    RequestAdditionalCollateral = 2
}

public sealed record VerifyCollateralCommand(
    Guid CollateralId,
    CollateralVerificationDecision Decision,
    string? Remark)
    : IRequest;