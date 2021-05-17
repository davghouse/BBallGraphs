using BBallGraphs.AzureStorage.Helpers;
using BBallGraphs.AzureStorage.Rows;
using BBallGraphs.AzureStorage.SyncResults;
using BBallGraphs.BasketballReferenceScraper;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallGraphs.AzureStorage
{
    public class TableService
    {
        private readonly CloudTable _playerFeedsTable;
        private readonly CloudTable _playersTable;
        private readonly CloudTable _gamesTable;

        public TableService(string connectionString,
            string playerFeedsTableName = "BBallGraphsPlayerFeeds",
            string playersTableName = "BBallGraphsPlayers",
            string gamesTableName = "BBallGraphsGames")
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();

            _playerFeedsTable = tableClient.GetTableReference(playerFeedsTableName);
            _playersTable = tableClient.GetTableReference(playersTableName);
            _gamesTable = tableClient.GetTableReference(gamesTableName);
        }

        public TableService(CloudTable playerFeedsTable, CloudTable playersTable, CloudTable gamesTable)
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

        public async Task<IReadOnlyList<PlayerRow>> GetPlayerRows()
        {
            var query = new TableQuery<PlayerRow>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "0"));

            return (await _playersTable.ExecuteQueryAsync(query))
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

        public async Task UpdatePlayerFeedsTable(PlayerFeedsSyncResult syncResult)
            => await _playerFeedsTable.ExecuteBatchesAsync(syncResult.DefunctPlayerFeedRows.Select(TableOperation.Delete)
                .Concat(syncResult.NewPlayerFeedRows.Select(TableOperation.Insert)));

        public async Task UpdatePlayersTable(PlayersSyncResult syncResult)
            => await _playersTable.ExecuteBatchesAsync(syncResult.DefunctPlayerRows.Select(TableOperation.Delete)
                .Concat(syncResult.NewPlayerRows.Select(TableOperation.Insert))
                .Concat(syncResult.UpdatedPlayerRows.Select(TableOperation.Replace)));

        public async Task UpdateGamesTable(GamesSyncResult syncResult)
        {
            if (syncResult.DefunctGameRows
                .Concat(syncResult.NewGameRows)
                .Concat(syncResult.UpdatedGameRows)
                .Select(r => r.PlayerID).Distinct().Count() > 1)
                throw new SyncException("Can only update games for one player at a time.");

            await _gamesTable.ExecuteBatchesAsync(syncResult.DefunctGameRows.Select(TableOperation.Delete)
                .Concat(syncResult.NewGameRows.Select(TableOperation.Insert))
                .Concat(syncResult.UpdatedGameRows.Select(TableOperation.Replace)));
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
