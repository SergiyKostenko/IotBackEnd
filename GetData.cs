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
            CloudTable table = GetTable();
            List<MyTableEntity> allItems = await GetAllAsync(table);
            if (allItems.Count != 0) return new OkObjectResult(allItems);
            else return new BadRequestResult();
        }

        public static CloudTable GetTable()
        {
            string connectionString = Environment.GetEnvironmentVariable("StorageConnection");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();
            CloudTable mytable = cloudTableClient.GetTableReference("IotTable");
            return mytable;
        }

        public static async Task<List<MyTableEntity>> GetAllAsync(CloudTable table)
        {
            TableQuery<MyTableEntity> query = new TableQuery<MyTableEntity>()
  .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "raspberry01"));
            TableContinuationToken continuationToken = null;

            List<MyTableEntity> allRecords = new List<MyTableEntity>();
            do
            {
                var batch = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = batch.ContinuationToken;
                allRecords.AddRange(batch.Results);
            }
            while (continuationToken != null);
            return allRecords;
        }
    }


    public class MyTableEntity : TableEntity
    {
        public Int64 humidity { get; set; }
        public double temperature { get; set; }
        public bool isFlameDetected { get; set; }
    }
}
