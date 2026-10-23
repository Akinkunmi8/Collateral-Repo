using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Infrastructure.Integrations.Mocks;

/// <summary>Stands in for the Credit Risk Management System / CLOS until a real integration is wired up.</summary>
public class MockCreditRiskClient : ICreditRiskClient
{
    private static readonly Dictionary<string, CreditRiskDto> Fixtures = new()
    {
        ["CIF-000456"] = new CreditRiskDto("CIF-000456", "A2", 1.8m, DateTime.UtcNow.AddMonths(-1)),
        ["CIF-000789"] = new CreditRiskDto("CIF-000789", "B1", 4.2m, DateTime.UtcNow.AddMonths(-2)),
        ["CIF-001011"] = new CreditRiskDto("CIF-001011", "A3", 2.5m, DateTime.UtcNow.AddDays(-10))
    };

    public Task<CreditRiskDto?> GetRiskProfileAsync(string customerId, CancellationToken ct = default)
    {
        Fixtures.TryGetValue(customerId, out var risk);
        return Task.FromResult(risk);
    }
}
