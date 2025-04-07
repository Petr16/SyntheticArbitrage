namespace SyntheticArbitrage.API.Prometheus;

public interface IBtcUsdtPriceMetricsService
{
    void SetPrices(decimal quarterPrice, decimal biQuarterPrice);
}
