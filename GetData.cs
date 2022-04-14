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
using System.Globalization;

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
            List<ResponseItem> allItems = await StaticHelpers.GetAlltableItemAsyncResponseItem(table);


           var  filteredItems= 
             allItems
             .Where(i=> DateTime.ParseExact(i.RowKey, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture) >= DateTime.Now.AddDays(-7))
             .ToList();

            if (filteredItems.Count != 0)
            {
                string top = req.Query["top"];
                if (!String.IsNullOrWhiteSpace(top))
                {
                    int topint = Int32.Parse(top);
                    return new OkObjectResult(filteredItems.Take(topint));
                }
                else
                    return new OkObjectResult(filteredItems);
            }
            else return new BadRequestResult();
        }
    }
}
