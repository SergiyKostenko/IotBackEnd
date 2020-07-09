using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotBackEnd
{
   static class StaticHelpers
    {
        public static CloudTable GetTable()
        {
            string connectionString = Environment.GetEnvironmentVariable("StorageConnection");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();
            CloudTable mytable = cloudTableClient.GetTableReference("IotTable");
            return mytable;
        }

        public static async Task<List<MyTableEntity>> GetAltableItemAsync(CloudTable table)
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
}
