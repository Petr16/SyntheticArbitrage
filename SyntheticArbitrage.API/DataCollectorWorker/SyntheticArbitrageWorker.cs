using DataReadersLib;
using SyntheticArbitrage.Shared.Enums;
using Serilog;
using ILogger = Serilog.ILogger;
using Serilog.Events;

namespace DataCollectorWorker
{
    public class SyntheticArbitrageWorker : BackgroundService
    {
        private readonly HttpClient _httpClient;
        //private readonly ILogger<SyntheticArbitrageWorker> _logger;
        private readonly ILogger _logger;  // Используем Serilog.ILogger

        public SyntheticArbitrageWorker(ILogger logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var interval in new[] { KlineIntervalEnum.OneHour, KlineIntervalEnum.OneDay })
                {
                    for (ushort attempts = 1; attempts < 3; attempts++)//делаем две попытки,если первая провалилась
                    {
                        try
                        {
                            if (_logger.IsEnabled(LogEventLevel.Information))
                            {
                                _logger.Information("SyntheticArbitrageWorker running at: {time}", DateTimeOffset.Now);
                            }
                            await SyntheticArbitrageReader.PostDIffQBQPrices(_httpClient, interval, _logger);

                            break;
                        }
                        catch (Exception e)
                        {
                            _logger.Error("SyntheticArbitrageWorker fail, attempt " + attempts, e);
                        }
                    }
                    await Task.Delay(2 *1000);// 2sec итоговое время между двумя запросами, т.к. частота запроса к BinanceApi ограничивается 
                }
                await Task.Delay(15 * 1000, stoppingToken); // 15sec
            }
        }
    }
}
