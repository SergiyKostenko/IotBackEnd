using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace IotBackEnd
{
    public static class GenerateReports
    {
        [FunctionName("GenerateReports")]
        public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 0 10 * * *",RunOnStartup =true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            CloudTable table = StaticHelpers.GetTable("IotTable");
            List<MyTableEntity> allItems = await StaticHelpers.GetAlltableItemAsync(table);
            List<ResponseItem> result = ProcessItems(allItems);
        }

        private static List<ResponseItem> ProcessItems(List<MyTableEntity> allItems)
        {
            allItems.ForEach(i => {

                var date = i.Timestamp;
                string newvar = $"{date.Year}-{date.Month}-{date.Day}T{date.Hour}:00:00";
                i.RowKey = newvar;

            });

            List<string> DateRanges = allItems.Select(i => i.RowKey).Distinct().ToList();
            List<ResponseItem> result = new List<ResponseItem>();
            foreach (string item in DateRanges)
            {
                List<MyTableEntity> range = allItems.Where(i => i.RowKey == item).ToList();
                ResponseItem entity = new ResponseItem
                {
                    DeviceName = range.FirstOrDefault().PartitionKey,
                    Date = item,
                    humidity = (int)Math.Round(range.Average(i => i.humidity), 0),
                    temperature = (int)Math.Round(range.Average(i => i.temperature), 0),
                    isFlameDetected = range.Any(i => i.isFlameDetected),
                    
                    
                };
                result.Add(entity);
            }
            return result;
        }
    }
}
