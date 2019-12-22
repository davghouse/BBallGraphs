using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Syncer.Rows;
using BBallGraphs.Syncer.SyncResults;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BBallGraphs.Syncer.Tests
{
    [TestClass]
    public class AzureSyncServiceTests
    {
        private static int _tableNameDeduplicator = 0;
        private CloudTable _playerFeedsTable;
        private CloudTable _playersTable;
        private AzureSyncService _syncService;

        [TestInitialize]
        public async Task TestInitialize()
        {
            var account = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var tableClient = account.CreateCloudTableClient();

            _playerFeedsTable = tableClient.GetTableReference(
                $"BBallGraphsPlayerFeeds{Interlocked.Increment(ref _tableNameDeduplicator)}");
            await _playerFeedsTable.CreateIfNotExistsAsync();

            _playersTable = tableClient.GetTableReference(
                $"BBallGraphsPlayers{Interlocked.Increment(ref _tableNameDeduplicator)}");
            await _playersTable.CreateIfNotExistsAsync();

            _syncService = new AzureSyncService(_playerFeedsTable, _playersTable);
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await _playerFeedsTable.DeleteAsync();
            await _playersTable.DeleteAsync();
        }

        [TestMethod]
        public async Task GetPlayerFeedRows()
        {
            var playerFeedRows = await _syncService.GetPlayerFeedRows();

            Assert.AreEqual(0, playerFeedRows.Count);

            var playerFeeds = Enumerable.Range(1, 50)
                .Select(i => new PlayerFeed { Url = i.ToString() });
            var syncResult = new SyncPlayerFeedsResult(Enumerable.Empty<PlayerFeedRow>(), playerFeeds);
            await _syncService.UpdatePlayerFeedsTable(syncResult);
            playerFeedRows = await _syncService.GetPlayerFeedRows();

            Assert.AreEqual(50, playerFeedRows.Count);
            Assert.IsTrue(playerFeedRows.Zip(playerFeeds, (r, f) => r.Matches(f)).All(m => m));
        }

        [TestMethod]
        public async Task GetNextPlayerFeedRows()
        {
            var playerFeeds = Enumerable.Range(1, 50)
                .Select(i => new PlayerFeed { Url = i.ToString() })
                .ToArray();
            var syncResult = new SyncPlayerFeedsResult(Enumerable.Empty<PlayerFeedRow>(), playerFeeds);
            await _syncService.UpdatePlayerFeedsTable(syncResult);
            var nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();

            Assert.IsTrue(playerFeeds[0].Matches(nextPlayerFeedRow));

            var nextPlayerFeedRows = await _syncService.GetNextPlayerFeedRows(2, TimeSpan.Zero);

            Assert.IsTrue(playerFeeds[0].Matches(nextPlayerFeedRows[0]));
            Assert.IsTrue(playerFeeds[1].Matches(nextPlayerFeedRows[1]));
            Assert.AreEqual(2, nextPlayerFeedRows.Count);

            nextPlayerFeedRows = await _syncService.GetNextPlayerFeedRows(2, TimeSpan.FromDays(1));

            Assert.AreEqual(0, nextPlayerFeedRows.Count);
        }

        [TestMethod]
        public async Task GetPlayerRows()
        {
            var playerFeeds = Enumerable.Range(1, 50)
                .Select(i => new PlayerFeed { Url = i.ToString() })
                .ToArray();
            var playerRows = await _syncService.GetPlayerRows(playerFeeds[0]);

            Assert.AreEqual(0, playerRows.Count);

            var players = Enumerable.Range(1, 50)
                .SelectMany(i => Enumerable.Range(1, 5)
                    .Select(j => new Player
                    {
                        Url = $"{i}-{j}",
                        Name = $"{i}-{j}",
                        BirthDate = DateTime.UtcNow,
                        FeedUrl = playerFeeds[i - 1].Url,
                    }))
                .ToArray();
            var syncResult = new SyncPlayersResult(Enumerable.Empty<PlayerRow>(), players);
            await _syncService.UpdatePlayersTable(syncResult);
            playerRows = await _syncService.GetPlayerRows(playerFeeds[0]);

            Assert.AreEqual(5, playerRows.Count);
            CollectionAssert.AreEquivalent(
                new[] { "1-1", "1-2", "1-3", "1-4", "1-5" },
                playerRows.Select(r => r.Url).ToArray());

            playerRows = await _syncService.GetPlayerRows(playerFeeds[1]);

            Assert.AreEqual(5, playerRows.Count);
            CollectionAssert.AreEquivalent(
                new[] { "2-1", "2-2", "2-3", "2-4", "2-5" },
                playerRows.Select(r => r.Url).ToArray());

            playerRows = await _syncService.GetPlayerRows(playerFeeds[49]);

            Assert.AreEqual(5, playerRows.Count);
            CollectionAssert.AreEquivalent(
                new[] { "50-1", "50-2", "50-3", "50-4", "50-5" },
                playerRows.Select(r => r.Url).ToArray());

            playerRows = await _syncService.GetPlayerRows(new PlayerFeed { Url = "51" });
            Assert.AreEqual(0, playerRows.Count);
        }

        [TestMethod]
        public async Task UpdatePlayerFeedsTable()
        {
            var playerFeeds = Enumerable.Range(1, 110)
                .Select(i => new PlayerFeed { Url = i.ToString() });
            var syncResult = new SyncPlayerFeedsResult(Enumerable.Empty<PlayerFeedRow>(), playerFeeds);
            await _syncService.UpdatePlayerFeedsTable(syncResult);
            var playerFeedRows = await _syncService.GetPlayerFeedRows();

            CollectionAssert.AreEquivalent(
                playerFeeds.Select(f => f.Url).ToArray(),
                playerFeedRows.Select(r => r.Url).ToArray());
            Assert.IsTrue(playerFeedRows.All(r => r.PartitionKey == "0"));
            Assert.IsTrue(playerFeedRows.All(r => !r.LastSyncTimeUtc.HasValue));

            playerFeeds = Enumerable.Range(1, 112)
                .Select(i => new PlayerFeed { Url = i.ToString() });
            syncResult = new SyncPlayerFeedsResult(playerFeedRows, playerFeeds);
            await _syncService.UpdatePlayerFeedsTable(syncResult);
            playerFeedRows = await _syncService.GetPlayerFeedRows();

            CollectionAssert.AreEquivalent(
                playerFeeds.Select(f => f.Url).ToArray(),
                playerFeedRows.Select(r => r.Url).ToArray());
            Assert.IsTrue(playerFeedRows.All(r => r.PartitionKey == "0"));
            Assert.IsTrue(playerFeedRows.All(r => !r.LastSyncTimeUtc.HasValue));
            Assert.AreEqual($"112", playerFeedRows.OrderBy(r => r.RowKey).Last().Url);
            Assert.IsTrue(playerFeedRows.Zip(playerFeeds, (r, f) => r.Matches(f)).All(m => m));
        }

        [TestMethod]
        public async Task UpdatePlayersTable()
        {
            var playerFeeds = Enumerable.Range(1, 50)
                .Select(i => new PlayerFeed { Url = i.ToString() })
                .ToArray();
            var players = Enumerable.Range(1, 50)
                .SelectMany(i => Enumerable.Range(1, 5)
                    .Select(j => new Player
                    {
                        Url = $"{i}-{j}",
                        Name = $"{i}-{j}",
                        BirthDate = DateTime.UtcNow,
                        FeedUrl = playerFeeds[i - 1].Url,
                    }))
                .ToArray();
            var syncResult = new SyncPlayersResult(Enumerable.Empty<PlayerRow>(), players);
            await _syncService.UpdatePlayersTable(syncResult);

            var playerRowsFeed1 = await _syncService.GetPlayerRows(playerFeeds[0]);

            Assert.AreEqual(5, playerRowsFeed1.Count);

            var updatedPlayersFeed1 = playerRowsFeed1.Select(r => new Player
            {
                Url = r.Url,
                Name = r.Name + "-updated",
                FromYear = r.FromYear + 1,
                ToYear = r.ToYear + 1,
                BirthDate = r.BirthDate.AddYears(1),
                FeedUrl = r.FeedUrl,
            }).Concat(Enumerable.Range(6, 5)
                .Select(i => new Player
                {
                    Url = $"1-{i}",
                    Name = $"1-{i}",
                    BirthDate = DateTime.UtcNow,
                    FeedUrl = playerFeeds[0].Url,
                }))
            .ToArray();
            syncResult = new SyncPlayersResult(playerRowsFeed1, updatedPlayersFeed1);

            Assert.AreEqual(0, syncResult.DefunctPlayerRows.Count);
            Assert.AreEqual(5, syncResult.UpdatedPlayerRows.Count);
            Assert.AreEqual(5, syncResult.NewPlayers.Count);

            await _syncService.UpdatePlayersTable(syncResult);
            playerRowsFeed1 = await _syncService.GetPlayerRows(playerFeeds[0]);

            Assert.AreEqual(10, playerRowsFeed1.Count);
            Assert.IsTrue(playerRowsFeed1.Zip(updatedPlayersFeed1, (r, p) => r.Matches(p)).All(m => m));

            syncResult = new SyncPlayersResult(playerRowsFeed1, updatedPlayersFeed1);

            Assert.AreEqual(0, syncResult.DefunctPlayerRows.Count);
            Assert.AreEqual(0, syncResult.UpdatedPlayerRows.Count);
            Assert.AreEqual(0, syncResult.NewPlayers.Count);

            var playerRowsFeed5 = await _syncService.GetPlayerRows(playerFeeds[4]);

            Assert.AreEqual(5, playerRowsFeed5.Count);

            var updatedPlayersFeed5 = playerRowsFeed5.Select(r => new Player
            {
                Url = r.Url,
                Name = r.Name,
                BirthDate = r.BirthDate,
                FeedUrl = r.FeedUrl,
            }).Concat(Enumerable.Range(6, 1)
                .Select(i => new Player
                {
                    Url = $"5-{i}",
                    Name = $"5-{i}",
                    BirthDate = DateTime.UtcNow,
                    FeedUrl = playerFeeds[4].Url,
                }))
            .ToArray();
            updatedPlayersFeed5[2].Name += "-updated";
            syncResult = new SyncPlayersResult(playerRowsFeed5, updatedPlayersFeed5);

            Assert.AreEqual(0, syncResult.DefunctPlayerRows.Count);
            Assert.AreEqual(1, syncResult.UpdatedPlayerRows.Count);
            Assert.AreEqual(1, syncResult.NewPlayers.Count);

            await _syncService.UpdatePlayersTable(syncResult);
            playerRowsFeed1 = await _syncService.GetPlayerRows(playerFeeds[0]);
            playerRowsFeed5 = await _syncService.GetPlayerRows(playerFeeds[4]);

            Assert.AreEqual(10, playerRowsFeed1.Count);
            Assert.IsTrue(playerRowsFeed1.Zip(updatedPlayersFeed1, (r, p) => r.Matches(p)).All(m => m));

            Assert.AreEqual(6, playerRowsFeed5.Count);
            Assert.IsTrue(playerRowsFeed5.Zip(updatedPlayersFeed5, (r, p) => r.Matches(p)).All(m => m));
            Assert.AreEqual(syncResult.UpdatedPlayerRows.Single().RowKey, playerRowsFeed5[2].RowKey); // Same after the update.
        }

        [TestMethod]
        public async Task RequeuePlayerFeedRow()
        {
            var playerFeeds = Enumerable.Range(1, 3)
                .Select(i => new PlayerFeed { Url = i.ToString() })
                .ToArray();
            var syncResult = new SyncPlayerFeedsResult(Enumerable.Empty<PlayerFeedRow>(), playerFeeds);
            await _syncService.UpdatePlayerFeedsTable(syncResult);

            var nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();

            Assert.AreEqual("1", nextPlayerFeedRow.Url);
            Assert.IsNull(nextPlayerFeedRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerFeedRow.LastSyncWithChangesTimeUtc);

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, false);
            nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();

            Assert.AreEqual("2", nextPlayerFeedRow.Url);
            Assert.IsNull(nextPlayerFeedRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerFeedRow.LastSyncWithChangesTimeUtc);

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, true);
            nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();

            Assert.AreEqual("3", nextPlayerFeedRow.Url);
            Assert.IsNull(nextPlayerFeedRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerFeedRow.LastSyncWithChangesTimeUtc);

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, true);
            nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();

            Assert.AreEqual("1", nextPlayerFeedRow.Url);
            Assert.IsNotNull(nextPlayerFeedRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerFeedRow.LastSyncWithChangesTimeUtc);

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, true);
            nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();

            Assert.AreEqual("2", nextPlayerFeedRow.Url);
            Assert.IsNotNull(nextPlayerFeedRow.LastSyncTimeUtc);
            Assert.IsNotNull(nextPlayerFeedRow.LastSyncWithChangesTimeUtc);

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, true);
            var nextPlayerFeedRows = await _syncService.GetNextPlayerFeedRows(3, TimeSpan.Zero);

            CollectionAssert.AreEqual(
                new[] { "3", "1", "2" },
                nextPlayerFeedRows.Select(r => r.Url).ToArray());
        }
    }
}
