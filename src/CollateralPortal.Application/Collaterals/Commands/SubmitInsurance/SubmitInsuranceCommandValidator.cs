using FluentValidation;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitInsurance;

public sealed class SubmitInsuranceCommandValidator
    : AbstractValidator<SubmitInsuranceCommand>
{
    public SubmitInsuranceCommandValidator()
    {
        RuleFor(x => x.CollateralId)
            .NotEmpty();

        RuleFor(x => x.PolicyNumber)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Insurer)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.CoverageAmount)
            .GreaterThan(0);

        RuleFor(x => x.PremiumAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PolicyStartDate)
            .NotEmpty();

        RuleFor(x => x.RenewalCadence)
            .IsInEnum();
    }
}