using CollateralPortal.Domain.Enums;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitInsurance;

public sealed record SubmitInsuranceCommand(
    Guid CollateralId,
    string PolicyNumber,
    string Insurer,
    decimal CoverageAmount,
    decimal PremiumAmount,
    DateTime PolicyStartDate,
    InsuranceRenewalCadence RenewalCadence)
    : IRequest<Unit>;
