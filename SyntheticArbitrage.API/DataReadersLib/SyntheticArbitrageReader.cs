using SyntheticArbitrage.Shared.Enums;
using SyntheticArbitrage.Shared.Model;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Serilog;

namespace DataReadersLib;

public static class SyntheticArbitrageReader
{
    private static readonly string _apiUrl = "https://localhost:7071";// /api/Binance/price/qbq-diff

    public static async Task PostDIffQBQPrices(HttpClient httpClient, KlineIntervalEnum klineIntervalEnum, Serilog.ILogger _logger)
    {
        string requestUri = $"{_apiUrl}/api/Binance/price/qbq-diff" + $"?pairRequest=BTCUSDT&intervalRequest={klineIntervalEnum}&limitRequest=1";
        HttpRequestMessage? request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _logger.Information("Posting price diff for interval: {Interval}", klineIntervalEnum.ToIntervalString());

        HttpResponseMessage response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            BinanceQBQDiffPriceResponseAM? diffResponse = await response.Content.ReadFromJsonAsync<BinanceQBQDiffPriceResponseAM>();
            if (diffResponse != null)
            {
                Console.WriteLine($"diffResponse BTCUSDT quarters price {diffResponse.PriceDiff} by interval {klineIntervalEnum.ToIntervalString()} calculated succefull");
            }
        }
    }
}
