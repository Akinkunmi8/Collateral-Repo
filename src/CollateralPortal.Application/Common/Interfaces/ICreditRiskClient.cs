using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Application.Common.Interfaces;

/// <summary>Port to the Credit Risk Management System / CLOS. Mocked for now.</summary>
public interface ICreditRiskClient
{
    Task<CreditRiskDto?> GetRiskProfileAsync(string customerId, CancellationToken ct = default);
}
