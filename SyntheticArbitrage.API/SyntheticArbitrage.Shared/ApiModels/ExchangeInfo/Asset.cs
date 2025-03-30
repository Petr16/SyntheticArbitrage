using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.ApiModels.ExchangeInfo;

public class Asset
{
    public string AssetName { get; set; }
    public bool MarginAvailable { get; set; }
    public string AutoAssetExchange { get; set; }
}
