using FluentValidation;

namespace CollateralPortal.Application.Collaterals.Commands.CreateCollateral;

public class CreateCollateralCommandValidator : AbstractValidator<CreateCollateralCommand>
{
    public CreateCollateralCommandValidator()
    {
        RuleFor(x => x.LoanContractId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Region).IsInEnum();
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
    }
}
