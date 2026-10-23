using CollateralPortal.Application.Common.Models;

namespace CollateralPortal.Application.Common.Interfaces;

/// <summary>
/// Port to NGX Market Data. Backed by a mock today; swap to the real
/// adapter once an NGX API subscription/key is provisioned.
/// </summary>
public interface IMarketDataClient
{
    Task<MarketPriceDto?> GetLatestPriceAsync(string ticker, CancellationToken ct = default);
}
