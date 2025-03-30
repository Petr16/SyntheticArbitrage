using SyntheticArbitrage.Shared.Enums;

namespace SyntheticArbitrage.Shared.ApiModels.Kline;

public class BinanceKlineRequest
{
    public required string Symbol { get; set; }
    /// <summary>
    /// Candle time-interval
    /// </summary>
    public KlineIntervalEnum Interval { get; set; }
    /// <summary>
    /// Candles amount
    /// </summary>
    public int Limit { get; set; }
}
