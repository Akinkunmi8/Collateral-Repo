using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.ResubmitValuation;

public sealed record ResubmitValuationCommand(
    Guid CollateralId,
    string Response)
    : IRequest;