using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace CollateralPortal.Infrastructure.Integrations.Real;

/// <summary>
/// TODO: implement once NGX Market Data API access is provisioned
/// (subscription via NGX — see https://marketdataapiv3.ngxgroup.com).
/// Wire this up in DependencyInjection.cs by swapping the
/// IMarketDataClient registration from Mock to this adapter, gated by
/// an "Integrations:MarketData:UseMock" config flag.
/// </summary>
public class MarketDataAdapter : IMarketDataClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MarketDataAdapter> _logger;

    public MarketDataAdapter(HttpClient httpClient, ILogger<MarketDataAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<MarketPriceDto?> GetLatestPriceAsync(string ticker, CancellationToken ct = default)
    {
        throw new NotImplementedException(
            "Real NGX market data integration pending API subscription/key. " +
            "Use MockMarketDataClient until then.");
    }
}
