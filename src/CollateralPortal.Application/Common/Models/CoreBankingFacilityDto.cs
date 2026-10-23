namespace CollateralPortal.Application.Common.Models;

/// <summary>Shape returned by the (mocked) Vision core banking system.</summary>
public record CoreBankingFacilityDto(
    string LoanContractId,
    decimal OutstandingBalance,
    DateTime NextRepaymentDate,
    bool FullyRepaid,
    string Branch,
    string Region);
