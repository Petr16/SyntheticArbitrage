using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.RabbitMQModels;

public class TickerPriceRequest
{
    public string Pair { get; set; }
    public bool IsQuarter { get; set; }
}
