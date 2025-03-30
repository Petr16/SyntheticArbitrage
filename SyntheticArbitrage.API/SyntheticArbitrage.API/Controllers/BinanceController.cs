using Microsoft.AspNetCore.Mvc;
using SyntheticArbitrage.Infrastructure.Services.Http;
using SyntheticArbitrage.Infrastructure.Utils;
using SyntheticArbitrage.Shared.ApiModels;
using SyntheticArbitrage.Shared.ApiModels.ExchangeInfo;
using SyntheticArbitrage.Shared.ApiModels.Kline;
using SyntheticArbitrage.Shared.ApiModels.Price;
using SyntheticArbitrage.Shared.Enums;

namespace SyntheticArbitrage.API.Controllers;

[Route("api/[controller]")]
public class BinanceController : Controller
{
    private readonly IBinanceHttpService _binanceHttpService;

    public BinanceController(IBinanceHttpService binanceHttpService)
    {
        _binanceHttpService = binanceHttpService;
    }

    [HttpGet("price")]
    public async Task<List<BinanceTickerPriceResponse>> GetPrice([FromQuery] string symbol)
    {
        BinanceExchangeInfoResponse? exchangeInfo = await _binanceHttpService.GetExchangeInfo();
        if (exchangeInfo == null)
            throw new Exception("Exchange info not found");

        SymbolInfo? quarter = exchangeInfo.Symbols
            .FirstOrDefault(s => s.Status == "TRADING"
                && s.Pair == "BTCUSDT"
                && s.ContractType == ContractTypeEnum.CurrentQuarter.ToTypeString());
        SymbolInfo? bi_quarter = exchangeInfo.Symbols
            .FirstOrDefault(s => s.Status == "TRADING"
                && s.Pair == "BTCUSDT"
                && s.ContractType == ContractTypeEnum.NextQuarter.ToTypeString());

        if (quarter == null)
            throw new Exception("BTCUSDT_QUARTER not found");
        if (bi_quarter == null)
            throw new Exception("BTCUSDT_BI-QUARTER not found");

        List<BinanceTickerPriceResponse> price = await _binanceHttpService.GetTickerPrices("BTCUSDT",isQuarter: true);

        BinanceKlineRequest quarterKlineRequest = new()
        {
            Symbol = quarter.Symbol,
            Interval = KlineIntervalEnum.OneHour,
            Limit = 24
        };
        List<BinanceKlineResponse> quarterCandles = await _binanceHttpService.GetCandlestickBySymbol(quarterKlineRequest);
        var lastQCandle = quarterCandles.LastOrDefault();
        BinanceTickerPriceResponse qPrice = new()
        {
            Symbol = quarter.Symbol + " (BTCUSDT_QUARTER)",
            Price = lastQCandle.Close,
            Time = DateTimeUtil.GetUnixTimestampMs(lastQCandle.CloseTime)
        };
        price.Add(qPrice);

        BinanceKlineRequest bi_quarterKlineRequest = new()
        {
            Symbol = bi_quarter.Symbol,
            Interval = KlineIntervalEnum.OneHour,
            Limit = 24
        };
        List<BinanceKlineResponse> bi_quarterCandles = await _binanceHttpService.GetCandlestickBySymbol(bi_quarterKlineRequest);
        var lastBQCandle = bi_quarterCandles.LastOrDefault();
        BinanceTickerPriceResponse bqPrice = new()
        {
            Symbol = bi_quarter.Symbol + " (BTCUSDT_BI-QUARTER)",
            Price = lastBQCandle.Close,
            Time = DateTimeUtil.GetUnixTimestampMs(lastBQCandle.CloseTime)
        };
        price.Add(bqPrice);

        return price;
    }

    [HttpGet("exchangeInfo")]
    public async Task<object> GetExchangeInfo()
    {
        var exchangeInfo = await _binanceHttpService.GetExchangeInfo();
        return exchangeInfo;
    }
}
