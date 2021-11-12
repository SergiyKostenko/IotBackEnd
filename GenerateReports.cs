using System;
using System.Collections.Generic;
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
        public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 0 10 * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            CloudTable table = StaticHelpers.GetTable("IotTable");
            List<MyTableEntity> allItems = await StaticHelpers.GetAlltableItemAsync(table, "raspberry01");
            List<ResponseItem> result = ProcessItems(allItems);
            await UpsertToTableAsync(result);
        }

        private static async Task UpsertToTableAsync(List<ResponseItem> allItems)
        {
  
            CloudTable table = StaticHelpers.GetTable("IotTableHourly");
            //TableBatchOperation batchOperation = new TableBatchOperation();
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
                var newDate = new DateTime(year:date.Year,month:date.Month,day:date.Day,hour: date.Hour,0,0);
                i.RowKey = newDate.ToString("dd/MM/yyyy HH:mm:ss");

            });

            List<string> DateRanges = allItems.Select(i => i.RowKey).Distinct().ToList();
            List<ResponseItem> result = new List<ResponseItem>();
            foreach (string item in DateRanges)
            {
                List<MyTableEntity> range = allItems.Where(i => i.RowKey == item).ToList();
                ResponseItem entity = new ResponseItem
                {
                    RowKey = item,
                    PartitionKey= DateTime.Now.ToString("MMMM"),
                    Date = item,
                    humidity = (int)Math.Round(range.Average(i => i.humidity), 0),
                    temperature = (int)Math.Round(range.Average(i => i.temperature), 0),
                    AQI= (int)Math.Round(range.Average(i => i.AQI), 0)
                };
                result.Add(entity);
            }
            return result;
        }
    }
}
