using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Infrastructure.Integrations.Mocks;

/// <summary>
/// Stands in for the NGX Market Data API until a subscription/API key is
/// provisioned. Response shape approximates NGX's daily trading summary /
/// OHLCV products — confirm exact fields once access is granted and update
/// <see cref="MarketPriceDto"/> + <see cref="Real.MarketDataAdapter"/> to match.
/// </summary>
public class MockMarketDataClient : IMarketDataClient
{
    private static readonly Dictionary<string, MarketPriceDto> Fixtures = new()
    {
        ["DANGCEM"] = new MarketPriceDto("DANGCEM", "Dangote Cement Plc", 415.50m, 410.00m, 418.00m, 408.50m, 1_250_000, DateTime.UtcNow.Date),
        ["MTNN"] = new MarketPriceDto("MTNN", "MTN Nigeria Communications Plc", 228.90m, 225.00m, 230.00m, 224.10m, 3_400_000, DateTime.UtcNow.Date),
        ["GTCO"] = new MarketPriceDto("GTCO", "Guaranty Trust Holding Co Plc", 58.25m, 57.80m, 59.00m, 57.50m, 5_120_000, DateTime.UtcNow.Date),
        ["ZENITHBANK"] = new MarketPriceDto("ZENITHBANK", "Zenith Bank Plc", 47.65m, 47.00m, 48.10m, 46.90m, 4_700_000, DateTime.UtcNow.Date)
    };

    public Task<MarketPriceDto?> GetLatestPriceAsync(string ticker, CancellationToken ct = default)
    {
        Fixtures.TryGetValue(ticker.ToUpperInvariant(), out var price);
        return Task.FromResult(price);
    }
}
