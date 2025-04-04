using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.Model;

public class BinanceTickerPriceAM
{
    public required string Symbol { get; set; }
    public decimal Price { get; set; }
    public DateTime Time { get; set; }
}
