using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SyntheticArbitrage.API.RabbitMQ;
using SyntheticArbitrage.DAL;
using SyntheticArbitrage.DAL.Entities;
using SyntheticArbitrage.Infrastructure.Services.Http;
using SyntheticArbitrage.Shared.ApiModels.ExchangeInfo;
using SyntheticArbitrage.Shared.ApiModels.Kline;
using SyntheticArbitrage.Shared.ApiModels.Price;
using SyntheticArbitrage.Shared.Enums;
using SyntheticArbitrage.Shared.Model;
using SyntheticArbitrage.Shared.RabbitMQModels;
using System.Collections.Generic;

namespace SyntheticArbitrage.API.Controllers;

[Route("api/[controller]")]
public class BinanceController : Controller
{
    private readonly IBinanceHttpService _binanceHttpService;
    private readonly IRabbitMQProducer _producer;
    private BinanceDbContext _dbContext;
    private readonly IMapper _mapper;

    public BinanceController(IBinanceHttpService binanceHttpService,
        IRabbitMQProducer producer,
        BinanceDbContext binanceDbContext,
        IMapper mapper)
    {
        _binanceHttpService = binanceHttpService;
        _producer = producer;
        _dbContext = binanceDbContext;
        _mapper = mapper;
    }

    /// <summary>
    /// Получить цены за квартальные и биквартальные пары и рассчитать разницу между ними
    /// </summary>
    /// <param name="pairReauest">Pair (forexample, BTCUSDT)</param>
    /// <param name="intervalRequest"></param>
    /// <param name="limitRequest"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("price/qbq-diff")]
    [SwaggerOperation(
        Summary = "Разница цен QUARTER - BI-QUARTER",
        Description = "Возвращает разницу цен между квартальным и би-квартальным фьючерсом и сохраняет в БД"
    )]
    public async Task<BinanceQBQDiffPriceResponseAM> PriceQBQDiff(
        [FromQuery, SwaggerParameter("Торговая пара, например BTCUSDT")] string pairRequest,
        [FromQuery, SwaggerParameter("Интервал свечей: 0(1m),1(3m),2(5m),3(15m),4(30m),5(1h),6(2h),7(4h),8(6h),9(8h),10(12h),11(1d),12(3d),13(1w),14(1M)")] KlineIntervalEnum intervalRequest,
        [FromQuery, SwaggerParameter("Количество свечей")] int limitRequest)
    {
        if (string.IsNullOrWhiteSpace(pairRequest))
            throw new Exception("Need correct pair");

        BinanceExchangeInfoResponse? exchangeInfo = await _binanceHttpService.GetExchangeInfo();
        if (exchangeInfo == null)
            throw new Exception("Exchange info not found");

        SymbolInfo? quarter = exchangeInfo.Symbols
            .FirstOrDefault(s => s.Status == "TRADING"
                && s.Pair == pairRequest//"BTCUSDT"
                && s.ContractType == ContractTypeEnum.CurrentQuarter.ToTypeString());
        SymbolInfo? bi_quarter = exchangeInfo.Symbols
            .FirstOrDefault(s => s.Status == "TRADING"
                && s.Pair == pairRequest//"BTCUSDT"
                && s.ContractType == ContractTypeEnum.NextQuarter.ToTypeString());

        if (quarter == null || bi_quarter == null)
            throw new Exception("One or all futures not found");

        //List<BinanceTickerPriceResponse> tickerPriceResponse = await _binanceHttpService.GetTickerPrices(pairRequest, isQuarter: true);
        //List<BinanceTickerPriceAM> tickerPrice = _mapper.Map<List<BinanceTickerPriceAM>>(tickerPriceResponse);

        #region RabbitMQ Request-Response
        TickerPriceRequest request = new()
        {
            Pair = pairRequest,
            IsQuarter = true
        };

        List<BinanceTickerPriceResponse>? tickerPriceResponse = await _producer.SendMessageAndGetResponseAsync<
                TickerPriceRequest, List<BinanceTickerPriceResponse>>(request, "ticker_price_request");
        #endregion

        List<BinanceTickerPriceAM> tickerPrice = new(); 
        if(tickerPriceResponse != null && tickerPriceResponse.Count > 0)
            tickerPrice = _mapper.Map<List<BinanceTickerPriceAM>>(tickerPriceResponse);

        //дожидаемся выполнение обоих запросов
        var quarterCandlesTask = _binanceHttpService.GetCandlestickBySymbol(new()
        {
            Symbol = quarter.Symbol,
            Interval = intervalRequest,
            Limit = 1
        });
        var biQuarterCandlesTask = _binanceHttpService.GetCandlestickBySymbol(new()
        {
            Symbol = bi_quarter.Symbol,
            Interval = intervalRequest,
            Limit = 1
        });

        await Task.WhenAll(quarterCandlesTask, biQuarterCandlesTask);

        // Обрабатываем результаты
        BinanceKlineResponse lastQCandle = await GetLastAvailableCandle(
            quarterCandlesTask.Result,
            quarter.Symbol,
            KlineIntervalEnum.OneDay
        );

        BinanceKlineResponse lastBQCandle = await GetLastAvailableCandle(
            biQuarterCandlesTask.Result,
            bi_quarter.Symbol,
            KlineIntervalEnum.OneDay
        );

        // формируем результаты
        BinanceTickerPriceAM qPrice = new()
        {
            Symbol = quarter.Symbol,
            Price = lastQCandle?.Close > 0
                ? lastQCandle.Close
                : tickerPrice.FirstOrDefault(tp => tp.Symbol == quarter.Symbol)?.Price ?? 0,
            Time = lastQCandle?.CloseTime ?? DateTime.UtcNow
        };

        BinanceTickerPriceAM bqPrice = new()
        {
            Symbol = bi_quarter.Symbol,
            Price = lastBQCandle?.Close > 0
                ? lastBQCandle.Close
                : tickerPrice.FirstOrDefault(tp => tp.Symbol == quarter.Symbol)?.Price ?? 0,
            Time = lastBQCandle?.CloseTime ?? DateTime.UtcNow
        };

        // сохраняем в БД
        var priceDiff = new BinanceQBQDiffPrice
        {
            QuarterSymbol = qPrice.Symbol,
            BiQuarterSymbol = bqPrice.Symbol,
            QuarterPrice = qPrice.Price,
            BiQuarterPrice = bqPrice.Price,
            PriceDiff = qPrice.Price - bqPrice.Price,
            TimestampUtc = DateTime.UtcNow,
            Interval = (int)intervalRequest
        };

        _dbContext.BinanceQBQDiffPrices.Add(priceDiff);
        await _dbContext.SaveChangesAsync();

        // Отправляем в RabbitMQ
        //await _producer.SendMessageAsync(priceDiff, "BTCUSDT_QUARTER_diff_calc_queue");

        return _mapper.Map<BinanceQBQDiffPriceResponseAM>(priceDiff);
    }

    /// <summary>
    /// Получить список пар и информацию о них.
    /// </summary>
    /// <returns></returns>
    [HttpGet("exchangeInfo")]
    public async Task<object> GetExchangeInfo()
    {
        var exchangeInfo = await _binanceHttpService.GetExchangeInfo();
        return exchangeInfo;
    }

    /// <summary>
    /// Получить последнюю доступную цену для пары
    /// </summary>
    /// <param name="candles"></param>
    /// <param name="symbol"></param>
    /// <param name="fallbackInterval"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<BinanceKlineResponse> GetLastAvailableCandle(
        List<BinanceKlineResponse> candles,
        string symbol,
        KlineIntervalEnum fallbackInterval)
    {
        BinanceKlineResponse? lastCandle = candles.LastOrDefault(c => c.Close >= 0);

        // Если данных за 1 час/день нет, ищем в fallback-интервале, увеличивая Limit
        if (lastCandle == null)
        {
            Console.WriteLine($"Not found data for {symbol}. Getting fallback interval data.");

            int limit = 1;
            List<BinanceKlineResponse> fallbackCandles = new(); //получаем 

            while (lastCandle == null)
            {
                BinanceKlineRequest fallbackRequest = new()
                {
                    Symbol = symbol,
                    Interval = fallbackInterval,
                    Limit = limit
                };

                fallbackCandles = await _binanceHttpService.GetCandlestickBySymbol(fallbackRequest);
                lastCandle = fallbackCandles.LastOrDefault(c => c.Close >= 0);

                // ограничиваем количество попыток и устанавливаем задержку,
                // чтобы избежать бесконечного цикла или превышения обращения к Binance API по количеству и времени
                if (lastCandle == null)
                {
                    Console.WriteLine($"No valid candle found for {symbol} with limit {limit}. To ");
                    limit++; // Увеличиваем limit на 1
                    await Task.Delay(500);
                }
                if (limit > 100)
                    throw new Exception($"No available data for {symbol} in fallback period after 100 attempts.");
            }

            if (lastCandle == null)
                throw new Exception($"No available data for {symbol} in fallback period.");
        }

        return lastCandle;
    }

}
