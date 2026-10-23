using CollateralPortal.Domain.Enums;
using FluentValidation;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitValuation;

public sealed class SubmitValuationCommandValidator
    : AbstractValidator<SubmitValuationCommand>
{
    public SubmitValuationCommandValidator()
    {
        RuleFor(x => x.CollateralId)
            .NotEmpty();

        RuleFor(x => x.Decision)
            .IsInEnum();

        RuleFor(x => x.Remark)
            .NotEmpty()
            .WithMessage(
                "A remark is required before the valuation can be routed.")
            .MaximumLength(1000);

        /*
         * Manual values are required only when approving
         * a manually entered valuation.
         */
        When(
            x => x.ManualEntry &&
                 x.Decision == ValuationDecision.Submit,
            () =>
            {
                RuleFor(x => x.ManualOpenMarketValue)
                    .NotNull()
                    .GreaterThan(0);

                RuleFor(x => x.ManualForcedSaleValue)
                    .NotNull()
                    .GreaterThan(0);

                RuleFor(x => x.ManualValuersName)
                    .NotEmpty();

                RuleFor(x => x.ManualValuationDate)
                    .NotNull();

                RuleFor(x => x.ReportBase64)
                    .NotEmpty()
                    .WithMessage(
                        "A valuation report must be attached for a manual valuation.");

                RuleFor(x => x.ReportContentType)
                    .NotEmpty();
            });
    }
}