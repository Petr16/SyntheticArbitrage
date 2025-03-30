using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.ApiModels.ExchangeInfo;

public class Filter
{
    public string FilterType { get; set; }
    public string TickSize { get; set; }
    public string MaxPrice { get; set; }
    public string MinPrice { get; set; }
    public string MinQty { get; set; }
    public string MaxQty { get; set; }
    public string StepSize { get; set; }
    public int? Limit { get; set; }
    public string Notional { get; set; }
    public string MultiplierUp { get; set; }
    public string MultiplierDown { get; set; }
    public string MultiplierDecimal { get; set; }
}
