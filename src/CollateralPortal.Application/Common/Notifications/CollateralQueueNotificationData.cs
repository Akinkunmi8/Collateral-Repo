namespace CollateralPortal.Application.Common.Notifications;

/// <summary>
/// Every field named in the "Notification on collateral queue" BRD
/// requirement. Populated from whatever has been captured so far — fields
/// not yet available at the collateral's current stage are left null and
/// rendered as "N/A" in the email body.
/// </summary>
public record CollateralQueueNotificationData(
    string FacilityType,
    string CollateralType,
    string CollateralDescription,
    string Currency,
    decimal? TotalCollateralValue,
    string? CollateralAddress,
    string CollateralLocationState,
    string AccountNumber,
    string ContractId,
    DateTime? LoanStartDate,
    DateTime? LoanMaturityDate,
    decimal? PrincipalAmount,
    decimal? ExposureLcy,
    string? ValuerName,
    DateTime? ValuationStartDate,
    DateTime? ValuationEndDate,
    string LegalStatus,
    string StateOfLocation,
    string Region,
    string? InsuranceName,
    decimal? PremiumAmount,
    decimal? SumInsuredAmount,
    DateTime? InsuranceStartDate,
    DateTime? InsuranceEndDate,
    string AssetType,
    string? CollateralOwner,
    string? RelationshipManagerName,
    string? BranchManagerName,
    DateTime? NextRepaymentDate,
    decimal? LoanExposure,
    string BorrowerName,
    string BranchName,
    string CifId,
    string? RiskRating)
{
    public string ValuationStatus => string.IsNullOrEmpty(ValuerName) ? "Pending" : "Done";
}
