using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Infrastructure.Integrations.Mocks;

/// <summary>
/// Stands in for a licensed Nigerian credit bureau (CRC Credit Bureau /
/// CreditRegistry / First Central) until real sandbox credentials are
/// issued. Response shape approximates a typical bureau report — confirm
/// against the real API Integration Kit once a bureau is confirmed, and
/// update <see cref="CreditBureauReportDto"/> + <see cref="Real.CreditBureauAdapter"/> to match.
/// </summary>
public class MockCreditBureauClient : ICreditBureauClient
{
    private static readonly Dictionary<string, CreditBureauReportDto> Fixtures = new()
    {
        ["CIF-000456"] = new CreditBureauReportDto("CIF-000456", "CRC Credit Bureau (mock)", 742, 3, 0, 4_100_000m, DateTime.UtcNow.AddDays(-5)),
        ["CIF-000789"] = new CreditBureauReportDto("CIF-000789", "CRC Credit Bureau (mock)", 611, 5, 1, 9_800_000m, DateTime.UtcNow.AddDays(-12)),
        ["CIF-001011"] = new CreditBureauReportDto("CIF-001011", "CRC Credit Bureau (mock)", 705, 2, 0, 1_250_000m, DateTime.UtcNow.AddDays(-3))
    };

    public Task<CreditBureauReportDto?> GetCreditReportAsync(string customerId, CancellationToken ct = default)
    {
        if (Fixtures.TryGetValue(customerId, out var report))
            return Task.FromResult<CreditBureauReportDto?>(report);

        // Unknown customer: return a thin, low-confidence mock rather than null,
        // mirroring how a bureau responds for a thin-file customer.
        return Task.FromResult<CreditBureauReportDto?>(
            new CreditBureauReportDto(customerId, "CRC Credit Bureau (mock)", 550, 0, 0, 0m, DateTime.UtcNow));
    }
}
