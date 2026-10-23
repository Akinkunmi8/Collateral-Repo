namespace CollateralPortal.Application.Common.Models;

/// <summary>
/// Shape modeled after the NGX Market Data API (OHLCV / daily trading
/// summary products). Confirm exact fields once NGX API access is
/// provisioned, and align the mock + this DTO accordingly.
/// </summary>
public record MarketPriceDto(
    string Ticker,
    string SecurityName,
    decimal LastPrice,
    decimal DayOpen,
    decimal DayHigh,
    decimal DayLow,
    long Volume,
    DateTime AsOfDate);
