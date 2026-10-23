using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Application.Common.Interfaces;

/// <summary>
/// Port to the real, already-built Valuation Portal. No mock by design —
/// this is meant to be wired to the real API as soon as the contract is
/// shared. A temporary mock can be added later if the real contract is
/// delayed, without changing any calling code.
/// </summary>
public interface IValuationPortalClient
{
    /// <summary>Looked up by account number (OperativeAcc), matching the real API's route — not loan contract ID.</summary>
    Task<ValuationReportDto?> GetValuationReportAsync(string accountNumber, CancellationToken ct = default);
}
