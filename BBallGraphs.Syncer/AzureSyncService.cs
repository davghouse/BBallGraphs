using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Syncer.Helpers;
using BBallGraphs.Syncer.Rows;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallGraphs.Syncer
{
    public class AzureSyncService
    {
        private const int _batchOperationLimit = 100;
        private readonly CloudTable _playerFeedsTable;

        public AzureSyncService()
            : this(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "PlayerFeeds")
        { }

        public AzureSyncService(string connectionString, string playerFeedsTableName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();

            _playerFeedsTable = tableClient.GetTableReference(playerFeedsTableName);
        }

        public AzureSyncService(CloudTable playerFeedsTable)
            => _playerFeedsTable = playerFeedsTable;

        public async Task<IReadOnlyList<PlayerFeedRow>> GetPlayerFeedRows()
        {
            var query = new TableQuery<PlayerFeedRow>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"));

            return (await _playerFeedsTable.ExecuteQueryAsync(query))
                .ToArray();
        }

        public async Task InsertPlayerFeedRows(IEnumerable<PlayerFeed> playerFeeds)
        {
            // Don't expect to ever hit the batch operation limit, but let's pretend.
            var batchTasks = new List<Task>();
            var batchOperation = new TableBatchOperation();
            foreach (var playerFeedRow in PlayerFeedRow.CreateRows(playerFeeds))
            {
                if (batchOperation.Count == _batchOperationLimit)
                {
                    batchTasks.Add(_playerFeedsTable.ExecuteBatchAsync(batchOperation));
                    batchOperation = new TableBatchOperation();
                }

                batchOperation.Insert(playerFeedRow);
            }
            batchTasks.Add(_playerFeedsTable.ExecuteBatchAsync(batchOperation));

            await Task.WhenAll(batchTasks);
        }
    }
}
