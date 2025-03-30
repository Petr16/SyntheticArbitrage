using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.ApiModels.ExchangeInfo;

public class BinanceExchangeInfoResponse
{
    public string Timezone { get; set; }
    public long ServerTime { get; set; }
    public string FuturesType { get; set; }
    public List<RateLimit> RateLimits { get; set; }
    public List<object> ExchangeFilters { get; set; }
    public List<Asset> Assets { get; set; }
    public List<SymbolInfo> Symbols { get; set; }
}
