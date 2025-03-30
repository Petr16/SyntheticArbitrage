using SyntheticArbitrage.Shared.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SyntheticArbitrage.Shared.ApiModels.ExchangeInfo;
using SyntheticArbitrage.Shared.ApiModels.Price;
using SyntheticArbitrage.Infrastructure.Utils;
using SyntheticArbitrage.Shared.ApiModels.Kline;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Globalization;
using SyntheticArbitrage.Shared.Enums;

namespace SyntheticArbitrage.Infrastructure.Services.Http;

public class BinanceHttpService : IBinanceHttpService
{
    private readonly HttpClient _httpClient;
    private readonly string FUTURES_API = "https://fapi.binance.com/fapi/v1";

    public BinanceHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    //public async Task<BinancePriceResponse> GetPriceAsync(string symbol)
    //{
    //    string requestUri = $"https://api.binance.com/api/v3/ticker/price?symbol={symbol}";
    //    HttpRequestMessage? request = new(HttpMethod.Get, requestUri);
    //    request.Headers.Accept.Add(
    //        new MediaTypeWithQualityHeaderValue("application/json"));
    //    HttpResponseMessage response = await _httpClient.SendAsync(request);

    //    if (response.IsSuccessStatusCode)
    //        throw new Exception($"Binance price for symbol {symbol} is not exist");

    //    BinancePriceResponse? binancePriceResponse =
    //            await response.Content.ReadFromJsonAsync<BinancePriceResponse>();

    //    return binancePriceResponse ?? new();
    //}

    public async Task<BinanceExchangeInfoResponse> GetExchangeInfo()
    {
        string requestUri = $"{FUTURES_API}/exchangeInfo";

        HttpRequestMessage? request = new(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"{requestUri} request failed");

        //object? exchangeInfo =
        //        await response.Content.ReadAsStringAsync();
        BinanceExchangeInfoResponse? exchangeInfo =
            await response.Content.ReadFromJsonAsync<BinanceExchangeInfoResponse>();

        return exchangeInfo ?? new();
    }

    public async Task<List<BinanceTickerPriceResponse>> GetTickerPrices(string? pair, bool isQuarter = false)
    {
        BinanceExchangeInfoResponse exchangeInfo = await GetExchangeInfo();

        var querySymbols = exchangeInfo.Symbols
            .Where(s => s.Status == "TRADING");

        if (!string.IsNullOrWhiteSpace(pair))
            querySymbols.Where(s => s.Pair == pair);

        if (isQuarter)
            querySymbols.Where(ct => ct.ContractType.Contains("QUARTER"));

        List<SymbolInfo> quarterSymbols = querySymbols.ToList();

        SymbolInfo? quarter = quarterSymbols.FirstOrDefault(s => s.ContractType.Contains("CURRENT"));
        SymbolInfo? bi_quarter = quarterSymbols.FirstOrDefault(s => s.ContractType.Contains("NEXT"));

        if (quarter == null || bi_quarter == null)
            throw new Exception();


        //https://developers.binance.com/docs/derivatives/usds-margined-futures/market-data/rest-api/Symbol-Price-Ticker#http-request
        //string requestUri2 = $"{FEATURES_API}/ticker/price?symbols=[\"{quarter.Symbol}\",\"{bi_quarter.Symbol}\"]";
        //т.к. для фьючерсов в апи нет параметра symbols, лучше получим их все и по ним получим нужные,
        //чем делать два запроса к апи
        string requestUri = $"{FUTURES_API}/ticker/price?symbol";

        HttpRequestMessage? request = new(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        List<BinanceTickerPriceResponse>? prices =
                await response.Content.ReadFromJsonAsync<List<BinanceTickerPriceResponse>>();

        if (prices == null)
            throw new Exception("Prices not found");

        List<BinanceTickerPriceResponse> quarterPrices = new();
        if (isQuarter)
        {
            BinanceTickerPriceResponse? quarterPrice = prices.FirstOrDefault(qp => qp.Symbol == quarter.Symbol);
            if (quarterPrice != null && quarterPrice.Price > 0)
                quarterPrices.Add(quarterPrice);
            else
                quarterPrices.Add(new BinanceTickerPriceResponse());//TODO: add from db

            BinanceTickerPriceResponse? bi_quarterPrice = prices.FirstOrDefault(qp => qp.Symbol == bi_quarter.Symbol);
            if (bi_quarterPrice != null && bi_quarterPrice.Price > 0)
                quarterPrices.Add(bi_quarterPrice);
            else
                quarterPrices.Add(new BinanceTickerPriceResponse());//TODO: add from db
        }


        return quarterPrices ?? [];
    }

    public async Task<List<BinanceKlineResponse>> GetCandlestickBySymbol(BinanceKlineRequest klineRequest)
    {
        //string symbol = "BTCUSDT_250627"; // Квартальный фьючерс
        //string interval = KlineIntervalEnum.OneHour.ToIntervalString();
        //int limit = 24; // Количество свечей (например, за сутки)

        //string requestUri = $"https://fapi.binance.com/fapi/v1/klines?symbol={symbol}&interval={interval}&limit={limit}";

        string requestUri = $"{FUTURES_API}/klines?symbol={klineRequest.Symbol}"
            + $"&interval={klineRequest.Interval.ToIntervalString()}"
            +$"&limit={klineRequest.Limit}";

        HttpRequestMessage? request = new(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        List<List<JsonElement>>? klinesObjects =
                await response.Content.ReadFromJsonAsync<List<List<JsonElement>>>();

        //List<List<string>>? klinesObjects2 =
        //        await response.Content.ReadFromJsonAsync<List<List<string>>>();

        if (klinesObjects == null)
            throw new Exception($"Klines not found");

        List<BinanceKlineResponse> klines = klinesObjects
            .Select(k => BinanceKlineResponse.FromArray(k))
            .ToList();

        return klines;
    }

}

//BTCUSDT_QUARTER
//{
//            "symbol": "BTCUSDT_250627",
//            "pair": "BTCUSDT",
//            "contractType": "CURRENT_QUARTER",
//            "deliveryDate": 1751011200000,
//            "onboardDate": 1735286400000,
//            "status": "TRADING",
//            "maintMarginPercent": "2.5000",
//            "requiredMarginPercent": "5.0000",
//            "baseAsset": "BTC",
//            "quoteAsset": "USDT",
//            "marginAsset": "USDT",
//            "pricePrecision": 1,
//            "quantityPrecision": 3,
//            "baseAssetPrecision": 8,
//            "quotePrecision": 8,
//            "underlyingType": "COIN",
//            "underlyingSubType": [],
//            "triggerProtect": "0.0500",
//            "liquidationFee": "0.010000",
//            "marketTakeBound": "0.05",
//            "maxMoveOrderLimit": 10000,
//            "filters": [
//                {
//                    "maxPrice": "1000000",
//                    "minPrice": "576.3",
//                    "filterType": "PRICE_FILTER",
//                    "tickSize": "0.1"
//                },
//                {
//    "filterType": "LOT_SIZE",
//                    "minQty": "0.001",
//                    "maxQty": "500",
//                    "stepSize": "0.001"
//                },
//                {
//    "filterType": "MARKET_LOT_SIZE",
//                    "stepSize": "0.001",
//                    "maxQty": "1",
//                    "minQty": "0.001"
//                },
//                {
//    "filterType": "MAX_NUM_ORDERS",
//                    "limit": 200
//                },
//                {
//    "filterType": "MAX_NUM_ALGO_ORDERS",
//                    "limit": 10
//                },
//                {
//    "filterType": "MIN_NOTIONAL",
//                    "notional": "5"
//                },
//                {
//    "filterType": "PERCENT_PRICE",
//                    "multiplierUp": "1.0500",
//                    "multiplierDecimal": "4",
//                    "multiplierDown": "0.9500"
//                }
//            ],
//            "orderTypes": [
//                "LIMIT",
//                "MARKET",
//                "STOP",
//                "STOP_MARKET",
//                "TAKE_PROFIT",
//                "TAKE_PROFIT_MARKET",
//                "TRAILING_STOP_MARKET"
//            ],
//            "timeInForce": [
//                "GTC",
//                "IOC",
//                "FOK",
//                "GTX",
//                "GTD"
//            ]
//        },


//BTCUSDT_BI-QUARTER
//{
//            "symbol": "BTCUSDT_250926",
//            "pair": "BTCUSDT",
//            "contractType": "NEXT_QUARTER",
//            "deliveryDate": 1758873600000,
//            "onboardDate": 1743148800000,
//            "status": "TRADING",
//            "maintMarginPercent": "2.5000",
//            "requiredMarginPercent": "5.0000",
//            "baseAsset": "BTC",
//            "quoteAsset": "USDT",
//            "marginAsset": "USDT",
//            "pricePrecision": 1,
//            "quantityPrecision": 3,
//            "baseAssetPrecision": 8,
//            "quotePrecision": 8,
//            "underlyingType": "COIN",
//            "underlyingSubType": [],
//            "triggerProtect": "0.0500",
//            "liquidationFee": "0.010000",
//            "marketTakeBound": "0.05",
//            "maxMoveOrderLimit": 10000,
//            "filters": [
//                {
//                    "maxPrice": "1000000",
//                    "tickSize": "0.1",
//                    "filterType": "PRICE_FILTER",
//                    "minPrice": "576.3"
//                },
//                {
//    "stepSize": "0.001",
//                    "filterType": "LOT_SIZE",
//                    "minQty": "0.001",
//                    "maxQty": "500"
//                },
//                {
//    "minQty": "0.001",
//                    "maxQty": "1",
//                    "stepSize": "0.001",
//                    "filterType": "MARKET_LOT_SIZE"
//                },
//                {
//    "filterType": "MAX_NUM_ORDERS",
//                    "limit": 200
//                },
//                {
//    "limit": 10,
//                    "filterType": "MAX_NUM_ALGO_ORDERS"
//                },
//                {
//    "notional": "5",
//                    "filterType": "MIN_NOTIONAL"
//                },
//                {
//    "filterType": "PERCENT_PRICE",
//                    "multiplierUp": "1.0500",
//                    "multiplierDown": "0.9500",
//                    "multiplierDecimal": "4"
//                }
//            ],
//            "orderTypes": [
//                "LIMIT",
//                "MARKET",
//                "STOP",
//                "STOP_MARKET",
//                "TAKE_PROFIT",
//                "TAKE_PROFIT_MARKET",
//                "TRAILING_STOP_MARKET"
//            ],
//            "timeInForce": [
//                "GTC",
//                "IOC",
//                "FOK",
//                "GTX",
//                "GTD"
//            ]
//        },

//Prices