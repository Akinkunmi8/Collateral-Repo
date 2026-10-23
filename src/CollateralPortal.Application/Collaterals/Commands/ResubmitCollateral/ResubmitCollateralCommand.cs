using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.ResubmitCollateral;

public sealed record ResubmitCollateralCommand(
    Guid CollateralId,
    string Response)
    : IRequest;