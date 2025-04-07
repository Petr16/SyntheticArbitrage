using System.Threading.Tasks;
using DexTab.DataCollectorLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // 
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true);

        IConfigurationRoot configuration = configurationBuilder.Build();


        HttpClient httpClient = new();

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger logger = factory.CreateLogger<Program>();

        // await DataCollector.CollectMints(httpClient, dbContext);
        // await DataCollector.CollectMintPricesInUSD(httpClient, dbContext, null);
        // await DataCollector.CollectPools(httpClient, dbContext);
        // await DataCollector.CollectPoolsFromCoinMarketCap(httpClient, dbContext);
        await DataCollector.CollectSolanaTransactionSignatures(httpClient, dbContext, logger);
        // await DataCollector.CollectCMCMetadataForMints(httpClient, dbContext);
        // await MintsDataCollector.CollectCryptoCurrencyFromCoinMarketCap(httpClient, dbContext, logger);
        // await DataCollector.CollectNewSolanaTransactionsDetails(httpClient, dbContext, logger);
    }
}