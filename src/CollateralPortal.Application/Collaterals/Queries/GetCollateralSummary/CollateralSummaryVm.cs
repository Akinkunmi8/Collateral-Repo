namespace CollateralPortal.Application.Collaterals.Queries.GetCollateralSummary;

public sealed record CollateralSummaryVm(
    int Total,
    int PendingVerification,
    int PendingValuation,
    int LegalPerfection,
    int Insurance,
    int OnHold,
    int Archived,
    int Released,
    int WithinTat,
    int BeyondTat);