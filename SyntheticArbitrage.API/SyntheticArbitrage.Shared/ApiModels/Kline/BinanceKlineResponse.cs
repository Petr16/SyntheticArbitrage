using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.ApiModels.Kline;

public class BinanceKlineResponse
{
    public DateTime OpenTime { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public DateTime CloseTime { get; set; }
    public decimal QuoteAssetVolume { get; set; }
    public int NumberOfTrades { get; set; }
    public decimal TakerBuyBaseAssetVolume { get; set; }
    public decimal TakerBuyQuoteAssetVolume { get; set; }
    public decimal Ignore { get; set; }

    public static BinanceKlineResponse FromArray(List<JsonElement> data)
    {
        return new BinanceKlineResponse
        {
            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(data[0].GetInt64()).UtcDateTime,
            Open = decimal.Parse(data[1].GetString()!, CultureInfo.InvariantCulture),
            High = decimal.Parse(data[2].GetString()!, CultureInfo.InvariantCulture),
            Low = decimal.Parse(data[3].GetString()!, CultureInfo.InvariantCulture),
            Close = decimal.Parse(data[4].GetString()!, CultureInfo.InvariantCulture),
            Volume = decimal.Parse(data[5].GetString()!, CultureInfo.InvariantCulture),
            CloseTime = DateTimeOffset.FromUnixTimeMilliseconds(data[6].GetInt64()).UtcDateTime,
            QuoteAssetVolume = decimal.Parse(data[7].GetString()!, CultureInfo.InvariantCulture),
            NumberOfTrades = data[8].GetInt32(),
            TakerBuyBaseAssetVolume = decimal.Parse(data[9].GetString()!, CultureInfo.InvariantCulture),
            TakerBuyQuoteAssetVolume = decimal.Parse(data[10].GetString()!, CultureInfo.InvariantCulture),
            Ignore = decimal.Parse(data[11].GetString()!, CultureInfo.InvariantCulture)
        };
    }
}
