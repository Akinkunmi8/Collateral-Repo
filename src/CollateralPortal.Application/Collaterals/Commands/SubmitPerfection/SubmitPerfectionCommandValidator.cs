using FluentValidation;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitPerfection;

public sealed class SubmitPerfectionCommandValidator: AbstractValidator<SubmitPerfectionCommand>
{
    public SubmitPerfectionCommandValidator()
    {
        RuleFor(x => x.CollateralId)
            .NotEmpty();

        RuleFor(x => x.RegistrationReference)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LegalUnit)
            .NotEmpty()
            .MaximumLength(100);

        When(x => !x.CollateralValid, () =>
        {
            RuleFor(x => x.InvalidReason)
                .NotEmpty()
                .MaximumLength(500);
        });
    }
}