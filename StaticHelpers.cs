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
        public static CloudTableClient cloudTableClient { get; private set; }

        static StaticHelpers()
        {
            string connectionString = Environment.GetEnvironmentVariable("StorageConnection");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            cloudTableClient = storageAccount.CreateCloudTableClient();
        }

        public static CloudTable GetTable(string tableName)
        {
            CloudTable mytable = cloudTableClient.GetTableReference(tableName);
            return mytable;
        }

        public static async Task<List<MyTableEntity>> GetAlltableItemAsync(CloudTable table, string PartitionKey)
        {
            TableQuery<MyTableEntity> query = new TableQuery<MyTableEntity>()
  .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey));
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

        public static async Task<List<ResponseItem>> GetAlltableItemResponseAsync(CloudTable table, string PartitionKey)
        {
            TableQuery<ResponseItem> query = new TableQuery<ResponseItem>()
  .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey));
            TableContinuationToken continuationToken = null;

            List<ResponseItem> allRecords = new List<ResponseItem>();
            do
            {
                var batch = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = batch.ContinuationToken;
                allRecords.AddRange(batch.Results);
            }
            while (continuationToken != null);
            return allRecords;
        }



        public static async Task<List<ResponseItem>> GetAlltableItemAsyncResponseItem(CloudTable table)
        {
            List<ResponseItem> responseItems = new List<ResponseItem>();
            //temp as raspberry is down
            //string PartitionKeyThisMonth = DateTime.Now.ToString("MMMM-yyyy");
            //string PartitionKeyLastMonth = DateTime.Now.AddMonths(-1).ToString("MMMM-yyyy");

            string PartitionKeyThisMonth = "April-2022";
            string PartitionKeyLastMonth = "March-2022";

            responseItems = await GetItemByPartionkey(table, PartitionKeyThisMonth);
            responseItems.AddRange(await GetItemByPartionkey(table, PartitionKeyLastMonth));
            return responseItems;
        }

        private static async Task<List<ResponseItem>> GetItemByPartionkey(CloudTable table, string PartitionKey)
        {
            TableQuery<ResponseItem> query = new TableQuery<ResponseItem>()
  .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey));
            TableContinuationToken continuationToken = null;

            List<ResponseItem> allRecords = new List<ResponseItem>();
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
