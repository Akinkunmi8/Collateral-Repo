using MediatR;

namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralWorkItem;

/// <summary>
/// The "select a workitem" view per the workstep UI spec: a read-only
/// Collateral Data tab plus whichever module tabs have data so far.
/// </summary>
public record GetCollateralWorkItemQuery(Guid CollateralId) : IRequest<CollateralWorkItemVm>;

public record CollateralDataTabVm(
    string LoanContractId,
    string AccountNumber,
    string CifId,
    string CustomerName,
    string? BranchName,
    string? FacilityType,
    string Currency,
    string CollateralType,
    string CollateralDescription,
    string Region,
    decimal ApprovedLimit,
    string? CollateralAddress,
    string? CollateralOwner,
    string? RelationshipManagerName,
    string? BranchManagerName);

public record ValuationTabVm(
    bool ManualEntry,
    decimal? OpenMarketValue,
    decimal? ForcedSaleValue,
    string? ValuersName,
    DateTime? ValuationDate,
    decimal ActualCoverageRatioPercent,
    decimal RequiredCoverageRatioPercent,
    bool CoverageAdequate,
    string Decision,
    string Remark);

public record PerfectionTabVm(
    string RegistrationReference,
    string LegalUnit,
    string Stage,
    string? QueryNotes,
    bool CollateralValid,
    string? InvalidReason);

public record InsuranceTabVm(
    string PolicyNumber,
    string Insurer,
    decimal CoverageAmount,
    decimal PremiumAmount,
    DateTime PolicyStartDate,
    DateTime PolicyExpiryDate,
    string RenewalCadence,
    bool RenewalReminderSent,
    bool OverdueNoticeSent);

public record ReleaseTabVm(
    bool LoanFullyRepaid,
    DateTime ReleaseDate,
    string ReleasedBy,
    string? Notes);

public record CollateralWorkItemVm(
    Guid Id,
    string CurrentStage,
    string Status,
    CollateralDataTabVm CollateralData,
    ValuationTabVm? Valuation,
    PerfectionTabVm? Perfection,
    InsuranceTabVm? Insurance,
    ReleaseTabVm? Release);
