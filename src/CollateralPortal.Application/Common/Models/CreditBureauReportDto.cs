namespace CollateralPortal.Application.Common.Models;

/// <summary>
/// Shape modeled after the licensed Nigerian credit bureaus (CRC Credit
/// Bureau / CreditRegistry / First Central). Field names are a reasonable
/// approximation — confirm against the real bureau's API Integration Kit
/// once one is selected, and adjust the mock + this DTO to match exactly.
/// </summary>
public record CreditBureauReportDto(
    string CustomerId,
    string BureauName,
    int CreditScore,
    int OpenFacilitiesCount,
    int DelinquentFacilitiesCount,
    decimal TotalOutstandingDebt,
    DateTime ReportDate);
