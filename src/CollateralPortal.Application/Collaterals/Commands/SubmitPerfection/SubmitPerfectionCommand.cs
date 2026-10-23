using CollateralPortal.Domain.Enums;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitPerfection;

public sealed record SubmitPerfectionCommand(
    Guid CollateralId,
    bool CollateralValid,
    string? InvalidReason,
    string RegistrationReference,
    string LegalUnit)
    : IRequest<Unit>;
