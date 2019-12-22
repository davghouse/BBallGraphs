using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Syncer.Helpers;
using BBallGraphs.Syncer.Rows;
using BBallGraphs.Syncer.SyncResults;
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
        private readonly CloudTable _playersTable;

        public AzureSyncService()
        {
            var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = account.CreateCloudTableClient();

            _playerFeedsTable = tableClient.GetTableReference("BBallGraphsPlayerFeeds");
            _playersTable = tableClient.GetTableReference("BBallGraphsPlayers");
        }

        public AzureSyncService(CloudTable playerFeedsTable, CloudTable playersTable)
        {
            _playerFeedsTable = playerFeedsTable;
            _playersTable = playersTable;
        }

        public async Task<IReadOnlyList<PlayerFeedRow>> GetPlayerFeedRows()
        {
            var query = new TableQuery<PlayerFeedRow>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"));

            return (await _playerFeedsTable.ExecuteQueryAsync(query))
                .ToArray();
        }

        public async Task<IReadOnlyList<PlayerFeedRow>> GetNextPlayerFeedRows(
            int rowLimit, TimeSpan minimumTimeSinceLastSync)
        {
            // Rows are always returned in ascending order by partition key, then row key. A row's row key
            // equals the ticks of the row's last sync time, so this returns the rows most needing a sync.
            string rowKeyCutoff = PlayerFeedRow.GetRowKey(DateTime.UtcNow.Subtract(minimumTimeSinceLastSync));
            var query = new TableQuery<PlayerFeedRow>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, rowKeyCutoff)))
                .Take(rowLimit);

            return (await _playerFeedsTable.ExecuteQueryAsync(query))
                .ToArray();
        }

        public async Task<IReadOnlyList<PlayerRow>> GetPlayerRows(IPlayerFeed playerFeed)
        {
            var query = new TableQuery<PlayerRow>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("FeedUrl", QueryComparisons.Equal, playerFeed.Url)));

            return (await _playersTable.ExecuteQueryAsync(query))
                .ToArray();
        }

        public async Task UpdatePlayerFeedsTable(SyncPlayerFeedsResult syncResult)
        {
            if (syncResult.DefunctPlayerFeedRows.Any())
                throw new SyncException("Defunct player feed rows are unexpected and require manual intervention.");

            var batchTasks = new List<Task>();
            var batchOperation = new TableBatchOperation();
            foreach (var playerFeedRow in PlayerFeedRow.CreateRows(syncResult.NewPlayerFeeds))
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

        public async Task UpdatePlayersTable(SyncPlayersResult syncResult)
        {
            if (syncResult.DefunctPlayerRows.Any())
                throw new SyncException("Defunct player rows require manual intervention.");

            var batchTasks = new List<Task>();
            var batchOperation = new TableBatchOperation();
            foreach (var playerRow in PlayerRow.CreateRows(syncResult.NewPlayers)
                .Concat(syncResult.UpdatedPlayerRows))
            {
                if (batchOperation.Count == _batchOperationLimit)
                {
                    batchTasks.Add(_playersTable.ExecuteBatchAsync(batchOperation));
                    batchOperation = new TableBatchOperation();
                }

                batchOperation.InsertOrReplace(playerRow);
            }
            batchTasks.Add(_playersTable.ExecuteBatchAsync(batchOperation));

            await Task.WhenAll(batchTasks);
        }

        public async Task RequeuePlayerFeedRow(PlayerFeedRow playerFeedRow, bool syncFoundChanges)
        {
            var batchOperation = new TableBatchOperation();
            batchOperation.Delete(playerFeedRow);
            batchOperation.Insert(playerFeedRow.CreateRequeuedRow(DateTime.UtcNow, syncFoundChanges));

            await _playerFeedsTable.ExecuteBatchAsync(batchOperation);
        }
    }
}
