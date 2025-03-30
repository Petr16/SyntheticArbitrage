using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.ApiModels.Price;

public class BinanceTickerPriceResponse
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public long Time { get; set; }
}
