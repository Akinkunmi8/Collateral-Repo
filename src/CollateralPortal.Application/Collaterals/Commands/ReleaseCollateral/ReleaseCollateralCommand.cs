using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.ReleaseCollateral;

public record ReleaseCollateralCommand(Guid CollateralId, string ReleasedBy, string? Notes) : IRequest<Unit>;
