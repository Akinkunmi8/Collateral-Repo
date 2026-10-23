namespace CollateralPortal.Application.Collaterals.Queries.GetArchivedCollaterals;

public sealed record ArchivedCollateralVm(
    Guid Id,
    string LoanContractId,
    string CustomerName,
    string AccountNumber,
    string CollateralType,
    string Region,
    decimal ApprovedLimit,
    string Currency,
    DateTime ReleaseDate,
    string ReleasedBy,
    bool LoanFullyRepaid,
    string? Notes);