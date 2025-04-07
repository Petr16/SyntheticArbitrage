using Prometheus;

namespace SyntheticArbitrage.API.Prometheus;

public class BtcUsdtPriceMetricsService : IBtcUsdtPriceMetricsService
{
    //https://github.com/prometheus-net/prometheus-net#gauges
    private static readonly Gauge QuarterPriceGauge = Metrics
        .CreateGauge("btcusdt_quarter_price", "BTCUSDT_QUARTER price");

    private static readonly Gauge BiQuarterPriceGauge = Metrics
        .CreateGauge("btcusdt_biquarter_price", "BTCUSDT_BI-QUARTER price");

    public void SetPrices(decimal quarterPrice, decimal biQuarterPrice)
    {
        //Inc() - increment
        //Dec() - decrement
        //Set() - setting value
        QuarterPriceGauge.Set((double)quarterPrice);
        BiQuarterPriceGauge.Set((double)biQuarterPrice);
    }
}
