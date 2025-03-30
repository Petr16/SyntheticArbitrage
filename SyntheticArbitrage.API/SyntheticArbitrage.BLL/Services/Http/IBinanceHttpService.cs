using SyntheticArbitrage.Shared.ApiModels;
using SyntheticArbitrage.Shared.ApiModels.ExchangeInfo;
using SyntheticArbitrage.Shared.ApiModels.Kline;
using SyntheticArbitrage.Shared.ApiModels.Price;

namespace SyntheticArbitrage.Infrastructure.Services.Http;

public interface IBinanceHttpService
{
    //Task<BinancePriceResponse> GetPriceAsync(string symbol);
    Task<BinanceExchangeInfoResponse> GetExchangeInfo();
    Task<List<BinanceTickerPriceResponse>> GetTickerPrices(string? pair, bool isQuarter = false);
    Task<List<BinanceKlineResponse>> GetCandlestickBySymbol(BinanceKlineRequest klineRequest);
}
