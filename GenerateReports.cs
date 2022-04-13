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
    public static class GenerateReports
    {
        [FunctionName("GenerateReports")]
        public static async Task RunAsync([TimerTrigger("0 0 10 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            CloudTable table = StaticHelpers.GetTable("IotTable");
            List<MyTableEntity> allItems = await StaticHelpers.GetAlltableItemAsync(table, "raspberry01");
        List<ResponseItem> result = ProcessItems(allItems);
            await UpsertToTableAsync(result);
            //clean up old
            await CleanUpOldItemsAsync(allItems);
        }

        private static async Task CleanUpOldItemsAsync(List<MyTableEntity> allItems)
        {
            CloudTable table = StaticHelpers.GetTable("IotTable");
            string partionKey = "raspberry01";
            foreach (var item in allItems)
            {
                TableOperation deleteOperation = TableOperation.Delete(item);
                await table.ExecuteAsync(deleteOperation);
            }
        }

        private static async Task UpsertToTableAsync(List<ResponseItem> allItems)
        {
            CloudTable table = StaticHelpers.GetTable("IotTableHourly");
            //    TableBatchOperation batchOperation = new TableBatchOperation();

            //allItems.ForEach(i => batchOperation.Add(TableOperation.InsertOrReplace(i)));
            //await table.ExecuteBatchAsync(batchOperation);

            foreach (var item in allItems)
            {
                await table.ExecuteAsync(TableOperation.InsertOrReplace(item));
            }

        }

        private static List<ResponseItem> ProcessItems(List<MyTableEntity> allItems)
        {
            allItems.ForEach(i =>
           {

               var date = i.Timestamp.DateTime;
               var newDate = new DateTime(year: date.Year, month: date.Month, day: date.Day, hour: date.Hour, 0, 0);
               i.DateString = newDate.ToString("dd/MM/yyyy HH:mm:ss");
           });

            List<string> DateRanges = allItems.Select(i => i.DateString).Distinct().ToList();
            List<ResponseItem> result = new List<ResponseItem>();
            foreach (string item in DateRanges)
            {
                List<MyTableEntity> range = allItems.Where(i => i.DateString == item).ToList();
                var date = DateTime.ParseExact(s: item, format: "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                ResponseItem entity = new ResponseItem
                {
                    RowKey = item,
                    PartitionKey = date.ToString("MMMM-yyyy"),
                    humidity = (int)Math.Round(range.Average(i => i.humidity), 0),
                    temperature = (int)Math.Round(range.Average(i => i.temperature), 0),
                    AQI = (int)Math.Round(range.Average(i => i.AQI), 0)
                };
                result.Add(entity);
            }
            return result;
        }
    }
}
