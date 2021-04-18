using BBallGraphs.BasketballReferenceScraper;
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
        private const int _batchOperationLimit = 100; // Azure's limit
        private readonly CloudTable _playerFeedsTable;
        private readonly CloudTable _playersTable;
        private readonly CloudTable _gamesTable;

        public AzureSyncService()
        {
            var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = account.CreateCloudTableClient();

            _playerFeedsTable = tableClient.GetTableReference("BBallGraphsPlayerFeeds");
            _playersTable = tableClient.GetTableReference("BBallGraphsPlayers");
            _gamesTable = tableClient.GetTableReference("BBallGraphsGames");
        }

        public AzureSyncService(CloudTable playerFeedsTable, CloudTable playersTable, CloudTable gamesTable)
        {
            _playerFeedsTable = playerFeedsTable;
            _playersTable = playersTable;
            _gamesTable = gamesTable;
        }

        public async Task<IReadOnlyList<PlayerFeedRow>> GetPlayerFeedRows()
        {
            var query = new TableQuery<PlayerFeedRow>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"));

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

        public async Task<IReadOnlyList<GameRow>> GetGameRows(IPlayer player, int season)
        {
            var query = new TableQuery<GameRow>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, player.ID),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForInt("Season", QueryComparisons.Equal, season)));

            return (await _gamesTable.ExecuteQueryAsync(query))
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

        public async Task<IReadOnlyList<PlayerRow>> GetNextPlayerRows(
            int rowLimit, TimeSpan minimumTimeSinceLastSync)
        {
            // Rows are always returned in ascending order by partition key, then row key. A row's row key
            // equals the ticks of the row's last sync time (or a later deprioritized time for players who
            // are probably retired), so this returns the rows most needing a sync.
            string rowKeyCutoff = PlayerRow.GetRowKey(DateTime.UtcNow.Subtract(minimumTimeSinceLastSync));
            var query = new TableQuery<PlayerRow>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, rowKeyCutoff)))
                .Take(rowLimit);

            return (await _playersTable.ExecuteQueryAsync(query))
                .ToArray();
        }

        public async Task UpdatePlayerFeedsTable(SyncPlayerFeedsResult syncResult)
        {
            if (syncResult.DefunctPlayerFeedRows.Any())
                throw new SyncException("Defunct player feed rows, manual intervention required.");

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
                throw new SyncException("Defunct player rows, manual intervention required.");

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

        public async Task UpdateGamesTable(SyncGamesResult syncResult)
        {
            if (syncResult.DefunctGameRows.Any())
                throw new SyncException("Defunct game rows, manual intervention required.");

            if (syncResult.NewGames.Select(g => g.PlayerID)
                .Concat(syncResult.UpdatedGameRows.Select(r => r.PlayerID))
                .Distinct().Count() > 1)
                throw new SyncException("Can only update games for one player at a time.");

            var batchTasks = new List<Task>();
            var batchOperation = new TableBatchOperation();
            foreach (var gameRow in GameRow.CreateRows(syncResult.NewGames)
                .Concat(syncResult.UpdatedGameRows))
            {
                if (batchOperation.Count == _batchOperationLimit)
                {
                    batchTasks.Add(_gamesTable.ExecuteBatchAsync(batchOperation));
                    batchOperation = new TableBatchOperation();
                }

                batchOperation.InsertOrReplace(gameRow);
            }
            batchTasks.Add(_gamesTable.ExecuteBatchAsync(batchOperation));

            await Task.WhenAll(batchTasks);
        }

        public async Task RequeuePlayerFeedRow(PlayerFeedRow playerFeedRow, bool syncFoundChanges)
        {
            var batchOperation = new TableBatchOperation();
            batchOperation.Delete(playerFeedRow);
            batchOperation.Insert(playerFeedRow.CreateRequeuedRow(DateTime.UtcNow, syncFoundChanges));

            await _playerFeedsTable.ExecuteBatchAsync(batchOperation);
        }

        public async Task RequeuePlayerRow(PlayerRow playerRow, int syncSeason, bool syncFoundChanges)
        {
            var batchOperation = new TableBatchOperation();
            batchOperation.Delete(playerRow);
            batchOperation.Insert(playerRow.CreateRequeuedRow(DateTime.UtcNow, syncSeason, syncFoundChanges));

            await _playersTable.ExecuteBatchAsync(batchOperation);
        }
    }
}
