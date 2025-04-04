using SyntheticArbitrage.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.RabbitMQModels;

public class TickerPriceResponse
{
    public List<BinanceTickerPriceAM> Prices { get; set; }
}
