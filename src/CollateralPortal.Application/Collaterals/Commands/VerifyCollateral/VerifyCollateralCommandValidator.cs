using FluentValidation;

namespace CollateralPortal.Application.Collaterals.Commands.VerifyCollateral;

public sealed class VerifyCollateralCommandValidator
    : AbstractValidator<VerifyCollateralCommand>
{
    public VerifyCollateralCommandValidator()
    {
        RuleFor(x => x.CollateralId)
            .NotEmpty();

        RuleFor(x => x.Decision)
            .IsInEnum();

        RuleFor(x => x.Remark)
            .MaximumLength(1000);

        RuleFor(x => x.Remark)
            .NotEmpty()
            .When(x =>
                x.Decision ==
                CollateralVerificationDecision
                    .RequestAdditionalCollateral)
            .WithMessage(
                "Remark is required when requesting additional collateral.");
    }
}