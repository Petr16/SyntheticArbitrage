namespace SyntheticArbitrage.DAL.Entities;

public class BinanceQBQDiffPrice
{
    /// <summary>
    /// Символ текущего квартального фьючерса
    /// </summary>
    public required string QuarterSymbol { get; set; }
    /// <summary>
    /// Символ би-квартального (следующего) фьючерса
    /// </summary>
    public required string BiQuarterSymbol { get; set; }
    public DateTime TimestampUtc { get; set; }
    /// <summary>
    /// Интервал свечей
    /// </summary>
    public int Interval { get; set; }
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
