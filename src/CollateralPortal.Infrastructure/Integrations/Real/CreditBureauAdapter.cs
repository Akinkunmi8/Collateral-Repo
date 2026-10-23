using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace CollateralPortal.Infrastructure.Integrations.Real;

/// <summary>
/// TODO: implement once a licensed bureau (CRC Credit Bureau / CreditRegistry /
/// First Central) is confirmed and the API Integration Kit is issued.
/// Typical onboarding: request the kit -> sandbox test -> go live.
/// Wire this up in DependencyInjection.cs by swapping the
/// ICreditBureauClient registration from Mock to this adapter, gated by
/// an "Integrations:CreditBureau:UseMock" config flag.
/// </summary>
public class CreditBureauAdapter : ICreditBureauClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CreditBureauAdapter> _logger;

    public CreditBureauAdapter(HttpClient httpClient, ILogger<CreditBureauAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<CreditBureauReportDto?> GetCreditReportAsync(string customerId, CancellationToken ct = default)
    {
        throw new NotImplementedException(
            "Real credit bureau integration pending API Integration Kit + sandbox credentials. " +
            "Use MockCreditBureauClient until then.");
    }
}
