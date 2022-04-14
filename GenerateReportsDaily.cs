using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace IotBackEnd
{
    public static class GenerateReportsDaily
    {
        [FunctionName("GenerateReportsDaily")]
        public static async Task RunAsync([TimerTrigger("0 50 23 * * * ")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            CloudTable table = StaticHelpers.GetTable("IotTableHourly");
            List<ResponseItem> allItems = await StaticHelpers.GetAlltableItemResponseAsync(table, DateTime.Now.ToString("MMMM-yyyy"));

            allItems.ForEach(i =>
            {
                var date  = DateTime.ParseExact(s: i.RowKey, format: "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                i.RowKey = date.Day.ToString();
            });
            List<string> DateRanges = allItems.Select(i => i.RowKey).Distinct().ToList();
            List<ResponseItem> result = new List<ResponseItem>();

            foreach (var item in DateRanges)
            {
                List<ResponseItem> range = allItems.Where(i => i.RowKey == item).ToList();

                ResponseItem entity = new ResponseItem
                {
                    PartitionKey = range[0].PartitionKey,
                    RowKey=item,
                    humidity = (int)Math.Round(range.Average(i => i.humidity), 0),
                    temperature = (int)Math.Round(range.Average(i => i.temperature), 0),
                    AQI = (int)Math.Round(range.Average(i => i.AQI), 0)
                };
                result.Add(entity);
            }

            CloudTable tableDaily = StaticHelpers.GetTable("IotTableDaily");
            foreach (var item in result)
            {
                await tableDaily.ExecuteAsync(TableOperation.InsertOrReplace(item));
            }

        }
    }
}
