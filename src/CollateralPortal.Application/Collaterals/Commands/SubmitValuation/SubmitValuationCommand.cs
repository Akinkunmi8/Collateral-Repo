using CollateralPortal.Domain.Enums;
using MediatR;

namespace CollateralPortal.Application.Collaterals.Commands.SubmitValuation;

public record SubmitValuationCommand(
    Guid CollateralId,

    // If true, use the Manual* fields below instead of calling the Valuation
    // Portal — for when the Collateral Team must key the data in by hand
    // because no completed report exists yet for the account.
    bool ManualEntry,
    decimal? ManualOpenMarketValue,
    decimal? ManualForcedSaleValue,
    string? ManualValuersName,
    DateTime? ManualValuationDate,

    // Mandatory before the workitem can route onward, per the workstep UI rule.
    string ReportBase64,
    string ReportContentType,

    // User-driven decision — coverage ratio informs it, doesn't dictate it.
    ValuationDecision Decision,
    string Remark) : IRequest<SubmitValuationResult>;

public record SubmitValuationResult(
    bool CoverageAdequate,
    decimal ActualCoverageRatioPercent,
    decimal RequiredCoverageRatioPercent,
    ValuationDecision Decision);
