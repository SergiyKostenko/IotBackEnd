using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace IotBackEnd
{
    public static class GetData
    {
        [FunctionName("GetData")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            CloudTable table = StaticHelpers.GetTable();
            List<MyTableEntity> allItems = await StaticHelpers.GetAltableItemAsync(table);
            List<ResponseItem> result = ProcessItems(allItems);
            if (result.Count != 0)
            {
                string top = req.Query["top"];
                if (String.IsNullOrWhiteSpace(top))
                {
                    int topint = Int32.Parse(top);
                    return new OkObjectResult(allItems.Take(topint));
                }
                else
                    return new OkObjectResult(allItems);
            }
               
            else return new BadRequestResult();
        }

        private static List<ResponseItem> ProcessItems(List<MyTableEntity> allItems)
        {
            allItems.ForEach(i => {
                var date = DateTime.Parse(i.RowKey);
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
                    humidity = (int)Math.Round( range.Average(i => i.humidity),0),
                    temperature = (int)Math.Round(range.Average(i => i.temperature), 0),
                    isFlameDetected=range.Any(i=>i.isFlameDetected)
                };
                result.Add(entity);
            }
            return result;
        }
    }
}
