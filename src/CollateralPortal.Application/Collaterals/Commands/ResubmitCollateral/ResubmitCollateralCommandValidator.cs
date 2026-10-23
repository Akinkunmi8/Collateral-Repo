using FluentValidation;

namespace CollateralPortal.Application.Collaterals.Commands.ResubmitCollateral;

public sealed class ResubmitCollateralCommandValidator
    : AbstractValidator<ResubmitCollateralCommand>
{
    public ResubmitCollateralCommandValidator()
    {
        RuleFor(x => x.CollateralId)
            .NotEmpty();

        RuleFor(x => x.Response)
            .NotEmpty()
            .WithMessage(
                "A response is required before resubmitting the collateral.")
            .MaximumLength(1000);
    }
}