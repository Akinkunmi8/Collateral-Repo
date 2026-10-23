using FluentValidation;

namespace CollateralPortal.Application.Collaterals.Commands.ResubmitValuation;

public sealed class ResubmitValuationCommandValidator: AbstractValidator<ResubmitValuationCommand>
{
    public ResubmitValuationCommandValidator()
    {
        RuleFor(x => x.CollateralId)
            .NotEmpty();

        RuleFor(x => x.Response)
            .NotEmpty()
            .WithMessage("A response is required.")
            .MaximumLength(1000);
    }
}