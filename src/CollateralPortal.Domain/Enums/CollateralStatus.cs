namespace CollateralPortal.Domain.Enums;

public enum CollateralStatus
{
    Draft = 1,
    PendingApproval = 2,
    InValuation = 3,
    AdditionalCollateralRequested = 4,
    InPerfection = 5,
    InInsurance = 6,
    OnHold = 7,
    ReleasePending = 8,
    Released = 9,
    Archived = 10,
    AdditionalValuationInformationRequested = 11
}
public enum TatStatus
{
    WithinTat = 1,
    BeyondTat = 2
}
