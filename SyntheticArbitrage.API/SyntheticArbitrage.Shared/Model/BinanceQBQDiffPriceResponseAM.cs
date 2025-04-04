using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntheticArbitrage.Shared.Model;

public class BinanceQBQDiffPriceResponseAM
{
    /// <summary>
    /// Символ текущего квартального фьючерса
    /// </summary>
    public string QuarterSymbol { get; set; } = string.Empty;
    /// <summary>
    /// Символ би-квартального (следующего) фьючерса
    /// </summary>
    public string BiQuarterSymbol { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    /// <summary>
    /// Цена квартального фьючерса
    /// </summary>
    public decimal QuarterPrice { get; set; }
    /// <summary>
    /// Цена би-квартального фьючерса
    /// </summary>
    public decimal BiQuarterPrice { get; set; }
    /// <summary>
    /// Разница цен между QuarterPrice и BiQuarterPrice
    /// </summary>
    public decimal PriceDiff { get; set; }
}
