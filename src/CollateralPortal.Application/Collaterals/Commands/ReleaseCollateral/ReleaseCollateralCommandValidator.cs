using FluentValidation;

namespace CollateralPortal.Application.Collaterals.Commands.ReleaseCollateral;

public sealed class ReleaseCollateralCommandValidator: AbstractValidator<ReleaseCollateralCommand>
{
    public ReleaseCollateralCommandValidator()
    {
        RuleFor(x => x.CollateralId)
            .NotEmpty();

        RuleFor(x => x.ReleasedBy)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x =>
                !string.IsNullOrWhiteSpace(x.Notes));
    }
}