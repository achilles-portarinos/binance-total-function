using System;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using moderndrummer.binance;
using Newtonsoft.Json;

namespace Moderndrummer.Binance
{
    public class BinanceTotalCalculator
    {
        //private const string TimeTrigger = "0 0 0 */1 * *";

        [FunctionName("BinanceTotalCalculator")]
        public async Task Run([TimerTrigger("%TimeTrigger%")] TimerInfo myTimer, ILogger log)
        {
            string apiKey = Environment.GetEnvironmentVariable("BinanceApiKey");
            string secret = Environment.GetEnvironmentVariable("BinanceApiSecret");
            string storageConnectionString = Environment.GetEnvironmentVariable("storageConnectionString");

            var tableClient = new TableClient(storageConnectionString, "binancetotals");
            var binanceConnector = new BinanceConnector(apiKey, secret);
            var totals = await binanceConnector.GetTotalBalances();
            var date = DateTime.UtcNow.Ticks;

            var earnings = new BinanceTotal
            {
                Date = date,
                Totals = JsonConvert.SerializeObject(totals),
                RowKey = date.ToString()
            };

            await tableClient.AddEntityAsync(earnings);
        }

        private class BinanceTotal : ITableEntity
        {
            public long Date { get; set; }
            public string Totals { get; set; }
            public string PartitionKey { get; set; } = "default";
            public string RowKey { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }

            public BinanceTotal()
            {
            }
        }
    }
}
