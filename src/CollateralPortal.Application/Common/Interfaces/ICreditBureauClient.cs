using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Application.Common.Interfaces;

/// <summary>
/// Port to a licensed Nigerian credit bureau (CRC / CreditRegistry / First
/// Central). Backed by a mock today; swap the DI registration to the real
/// adapter once an API Integration Kit + sandbox credentials are issued.
/// </summary>
public interface ICreditBureauClient
{
    Task<CreditBureauReportDto?> GetCreditReportAsync(string customerId, CancellationToken ct = default);
}
