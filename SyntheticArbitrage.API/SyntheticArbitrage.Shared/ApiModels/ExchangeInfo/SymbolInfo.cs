using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.ApiModels.ExchangeInfo;

public class SymbolInfo
{
    public string Symbol { get; set; }
    public string Pair { get; set; }
    public string ContractType { get; set; }
    public long DeliveryDate { get; set; }
    public long OnboardDate { get; set; }
    public string Status { get; set; }
    public string MaintMarginPercent { get; set; }
    public string RequiredMarginPercent { get; set; }
    public string BaseAsset { get; set; }
    public string QuoteAsset { get; set; }
    public string MarginAsset { get; set; }
    public int PricePrecision { get; set; }
    public int QuantityPrecision { get; set; }
    public int BaseAssetPrecision { get; set; }
    public int QuotePrecision { get; set; }
    public string UnderlyingType { get; set; }
    public List<string> UnderlyingSubType { get; set; }
    public string TriggerProtect { get; set; }
    public string LiquidationFee { get; set; }
    public string MarketTakeBound { get; set; }
    public int MaxMoveOrderLimit { get; set; }
    public List<Filter> Filters { get; set; }
    public List<string> OrderTypes { get; set; }
    public List<string> TimeInForce { get; set; }
}
