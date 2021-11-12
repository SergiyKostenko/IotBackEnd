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
            CloudTable table = StaticHelpers.GetTable("IotTableHourly");
            List<MyTableEntity> allItems = await StaticHelpers.GetAlltableItemAsync(table, DateTime.Now.ToString("MMMM"));


           var  filteredItems= 
             allItems.Where(i=> DateTime.Parse(i.RowKey)>=DateTime.Now.AddDays(-7)).ToList();

            if (filteredItems.Count != 0)
            {
                string top = req.Query["top"];
                if (!String.IsNullOrWhiteSpace(top))
                {
                    int topint = Int32.Parse(top);
                    return new OkObjectResult(allItems.Take(topint));
                }
                else
                    return new OkObjectResult(allItems);
            }
            else return new BadRequestResult();
        }
    }
}
