using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Infrastructure.Integrations.Mocks;

/// <summary>Stands in for Vision (core banking) until a real integration is wired up.</summary>
public class MockCoreBankingClient : ICoreBankingClient
{
    private static readonly Dictionary<string, CoreBankingFacilityDto> Fixtures = new()
    {
        ["LN-2026-00123"] = new CoreBankingFacilityDto("LN-2026-00123", 38_200_000m, DateTime.UtcNow.AddDays(20), false, "Victoria Island", "Lagos"),
        ["LN-2026-00456"] = new CoreBankingFacilityDto("LN-2026-00456", 0m, DateTime.UtcNow.AddDays(-1), true, "Kano Main", "Kano"),
        ["LN-2026-00789"] = new CoreBankingFacilityDto("LN-2026-00789", 60_000_000m, DateTime.UtcNow.AddDays(30), false, "Wuse II", "Abuja")
    };

    public Task<CoreBankingFacilityDto?> GetFacilityAsync(string loanContractId, CancellationToken ct = default)
    {
        Fixtures.TryGetValue(loanContractId, out var facility);
        return Task.FromResult(facility);
    }
}
