using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Scrapers.Helpers;
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
        private CloudTable _gamesTable;
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

            _gamesTable = tableClient.GetTableReference(
                $"BBallGraphsGames{Interlocked.Increment(ref _tableNameDeduplicator)}");
            await _gamesTable.CreateIfNotExistsAsync();

            _syncService = new AzureSyncService(_playerFeedsTable, _playersTable, _gamesTable);
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await _playerFeedsTable.DeleteAsync();
            await _playersTable.DeleteAsync();
            await _gamesTable.DeleteAsync();
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
                        ID = $"{i}-{j}",
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
                playerRows.Select(r => r.ID).ToArray());

            playerRows = await _syncService.GetPlayerRows(playerFeeds[1]);
            Assert.AreEqual(5, playerRows.Count);
            CollectionAssert.AreEquivalent(
                new[] { "2-1", "2-2", "2-3", "2-4", "2-5" },
                playerRows.Select(r => r.ID).ToArray());

            playerRows = await _syncService.GetPlayerRows(playerFeeds[49]);
            Assert.AreEqual(5, playerRows.Count);
            CollectionAssert.AreEquivalent(
                new[] { "50-1", "50-2", "50-3", "50-4", "50-5" },
                playerRows.Select(r => r.ID).ToArray());

            playerRows = await _syncService.GetPlayerRows(new PlayerFeed { Url = "51" });
            Assert.AreEqual(0, playerRows.Count);
        }

        [TestMethod]
        public async Task GetGameRows()
        {
            var players = Enumerable.Range(1, 10)
                .Select(i => new Player { ID = i.ToString(), FirstSeason = 2000, LastSeason = 2010 })
                .ToArray();
            var gameRows = await _syncService.GetGameRows(players[0], 2000);
            Assert.AreEqual(0, gameRows.Count);

            var games = Enumerable.Range(1, 10)
                .SelectMany(i => Enumerable.Range(1, 110)
                    .Select(j => new Game
                    {
                        ID = $"{players[i - 1].ID}-{j}",
                        PlayerID = players[i - 1].ID,
                        Season = 2000 + i - 1,
                        Date = new DateTime(2000 + i - 1, 1, 1).AddDays(j).AsUtc()
                    }))
                .ToArray();
            var syncResult = new SyncGamesResult(Enumerable.Empty<GameRow>(), games.Where(g => g.PlayerID == "1"));
            await _syncService.UpdateGamesTable(syncResult);
            syncResult = new SyncGamesResult(Enumerable.Empty<GameRow>(), games.Where(g => g.PlayerID == "5"));
            await _syncService.UpdateGamesTable(syncResult);

            gameRows = await _syncService.GetGameRows(players[0], 2000);
            Assert.AreEqual(110, gameRows.Count);
            CollectionAssert.AreEquivalent(
                Enumerable.Range(1, 110).Select(i => $"1-{i}").ToArray(),
                gameRows.Select(r => r.ID).ToArray());

            gameRows = await _syncService.GetGameRows(players[4], 2004);
            Assert.AreEqual(110, gameRows.Count);
            CollectionAssert.AreEquivalent(
                Enumerable.Range(1, 110).Select(i => $"5-{i}").ToArray(),
                gameRows.Select(r => r.ID).ToArray());

            gameRows = await _syncService.GetGameRows(players[4], 2005);
            Assert.AreEqual(0, gameRows.Count);
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
        public async Task GetNextPlayerRows()
        {
            var players = Enumerable.Range(1, 10)
                .Select(i => new Player { ID = i.ToString(), BirthDate = DateTime.UtcNow })
                .ToArray();
            var syncResult = new SyncPlayersResult(Enumerable.Empty<PlayerRow>(), players);
            await _syncService.UpdatePlayersTable(syncResult);

            var nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.IsTrue(players[0].Matches(nextPlayerRow));

            var nextPlayerRows = await _syncService.GetNextPlayerRows(2, TimeSpan.Zero);
            Assert.IsTrue(players[0].Matches(nextPlayerRows[0]));
            Assert.IsTrue(players[1].Matches(nextPlayerRows[1]));
            Assert.AreEqual(2, nextPlayerRows.Count);

            nextPlayerRows = await _syncService.GetNextPlayerRows(2, TimeSpan.FromDays(1));
            Assert.AreEqual(0, nextPlayerRows.Count);
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
                        ID = $"{i}-{j}",
                        Name = $"{i}-{j}",
                        BirthDate = DateTime.UtcNow,
                        FeedUrl = playerFeeds[i - 1].Url,
                    })).ToArray();
            var syncResult = new SyncPlayersResult(Enumerable.Empty<PlayerRow>(), players);
            await _syncService.UpdatePlayersTable(syncResult);

            var playerRowsFeed1 = await _syncService.GetPlayerRows(playerFeeds[0]);
            Assert.AreEqual(5, playerRowsFeed1.Count);

            var updatedPlayersFeed1 = playerRowsFeed1.Select(r => new Player
            {
                ID = r.ID,
                Name = r.Name + "-updated",
                FirstSeason = r.FirstSeason + 1,
                LastSeason = r.LastSeason + 1,
                BirthDate = r.BirthDate.AddYears(1),
                FeedUrl = r.FeedUrl,
            }).Concat(Enumerable.Range(6, 5)
                .Select(i => new Player
                {
                    ID = $"1-{i}",
                    Name = $"1-{i}",
                    BirthDate = DateTime.UtcNow,
                    FeedUrl = playerFeeds[0].Url,
                })).ToArray();

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
                ID = r.ID,
                Name = r.Name,
                BirthDate = r.BirthDate,
                FeedUrl = r.FeedUrl,
            }).Concat(Enumerable.Range(6, 1)
                .Select(i => new Player
                {
                    ID = $"5-{i}",
                    Name = $"5-{i}",
                    BirthDate = DateTime.UtcNow,
                    FeedUrl = playerFeeds[4].Url,
                })).ToArray();
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
            Assert.AreEqual(syncResult.UpdatedPlayerRows.Single().RowKey, playerRowsFeed5[2].RowKey);
        }

        [TestMethod]
        public async Task UpdateGamesTable()
        {
            var players = Enumerable.Range(1, 10)
                .Select(i => new Player { ID = i.ToString(), BirthDate = DateTime.UtcNow })
                .ToArray();
            var games = Enumerable.Range(1, 10)
                .SelectMany(i => Enumerable.Range(1, 110)
                    .Select(j => new Game
                    {
                        ID = $"{players[i - 1].ID}-{j}",
                        PlayerID = players[i - 1].ID,
                        Season = 2000 + i - 1,
                        Date = new DateTime(2000 + i - 1, 1, 1).AddDays(j).AsUtc(),
                        Points = 10,
                        TotalRebounds = 5
                    })).ToArray();
            var syncResult = new SyncGamesResult(Enumerable.Empty<GameRow>(), games.Where(g => g.PlayerID == "1"));
            await _syncService.UpdateGamesTable(syncResult);
            syncResult = new SyncGamesResult(Enumerable.Empty<GameRow>(), games.Where(g => g.PlayerID == "5"));
            await _syncService.UpdateGamesTable(syncResult);

            var gameRowsPlayer1Season2000 = await _syncService.GetGameRows(players[0], 2000);
            Assert.AreEqual(110, gameRowsPlayer1Season2000.Count);

            var updatedGamesPlayer1 = gameRowsPlayer1Season2000.Select(r => new Game
            {
                ID = r.ID,
                PlayerID = r.PlayerID,
                Season = r.Season,
                Date = r.Date,
                Points = r.Points + 15,
                TotalRebounds = r.TotalRebounds + 10
            }).Concat(Enumerable.Range(1, 80)
                .Select(i => new Game
                {
                    ID = $"{players[0].ID}-{110 + i}",
                    PlayerID = players[0].ID,
                    Season = 2001,
                    Date = new DateTime(2001, 1, 1).AddDays(i).AsUtc(),
                    Points = 15,
                    TotalRebounds = 10
                })).ToArray();

            syncResult = new SyncGamesResult(gameRowsPlayer1Season2000, updatedGamesPlayer1);
            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(110, syncResult.UpdatedGameRows.Count);
            Assert.AreEqual(80, syncResult.NewGames.Count);

            await _syncService.UpdateGamesTable(syncResult);
            gameRowsPlayer1Season2000 = await _syncService.GetGameRows(players[0], 2000);
            var gameRowsPlayer1Season2001 = await _syncService.GetGameRows(players[0], 2001);
            Assert.AreEqual(110, gameRowsPlayer1Season2000.Count);
            Assert.AreEqual(80, gameRowsPlayer1Season2001.Count);
            Assert.IsTrue(gameRowsPlayer1Season2000.Zip(updatedGamesPlayer1.Where(g => g.Season == 2000), (r, g) => r.Matches(g)).All(m => m));
            Assert.IsTrue(gameRowsPlayer1Season2001.Zip(updatedGamesPlayer1.Where(g => g.Season == 2001), (r, g) => r.Matches(g)).All(m => m));

            syncResult = new SyncGamesResult(gameRowsPlayer1Season2000.Concat(gameRowsPlayer1Season2001), updatedGamesPlayer1);
            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(0, syncResult.UpdatedGameRows.Count);
            Assert.AreEqual(0, syncResult.NewGames.Count);

            var gameRowsPlayer4Season2000 = await _syncService.GetGameRows(players[3], 2000);
            Assert.AreEqual(0, gameRowsPlayer4Season2000.Count);

            var gameRowsPlayer5Season2000 = await _syncService.GetGameRows(players[4], 2000);
            Assert.AreEqual(0, gameRowsPlayer5Season2000.Count);

            var gameRowsPlayer5Season2004 = await _syncService.GetGameRows(players[4], 2004);
            Assert.AreEqual(110, gameRowsPlayer5Season2004.Count);

            var updatedGamesPlayer5 = gameRowsPlayer5Season2004.Select((r, i) => new Game
            {
                ID = r.ID,
                PlayerID = r.PlayerID,
                Season = r.Season,
                Date = r.Date,
                Points = r.Points + (i % 2 == 0 ? 15 : 0),
                TotalRebounds = r.TotalRebounds + (i % 2 == 0 ? 10 : 0)
            }).Concat(Enumerable.Range(1, 80)
                .Select(i => new Game
                {
                    ID = $"{players[4].ID}-{110 + i}",
                    PlayerID = players[4].ID,
                    Season = 2005,
                    Date = new DateTime(2005, 1, 1).AddDays(i).AsUtc(),
                    Points = 15,
                    TotalRebounds = 10
                })).ToArray();

            syncResult = new SyncGamesResult(gameRowsPlayer5Season2004, updatedGamesPlayer5);
            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(55, syncResult.UpdatedGameRows.Count);
            Assert.AreEqual(80, syncResult.NewGames.Count);

            await _syncService.UpdateGamesTable(syncResult);
            gameRowsPlayer1Season2000 = await _syncService.GetGameRows(players[0], 2000);
            gameRowsPlayer1Season2001 = await _syncService.GetGameRows(players[0], 2001);
            gameRowsPlayer5Season2004 = await _syncService.GetGameRows(players[4], 2004);
            var gameRowsPlayer5Season2005 = await _syncService.GetGameRows(players[4], 2005);
            Assert.AreEqual(110, gameRowsPlayer1Season2000.Count);
            Assert.AreEqual(80, gameRowsPlayer1Season2001.Count);
            Assert.AreEqual(110, gameRowsPlayer5Season2004.Count);
            Assert.AreEqual(80, gameRowsPlayer5Season2005.Count);
            Assert.IsTrue(gameRowsPlayer1Season2000.Zip(updatedGamesPlayer1.Where(g => g.Season == 2000), (r, g) => r.Matches(g)).All(m => m));
            Assert.IsTrue(gameRowsPlayer1Season2001.Zip(updatedGamesPlayer1.Where(g => g.Season == 2001), (r, g) => r.Matches(g)).All(m => m));
            Assert.IsTrue(gameRowsPlayer5Season2004.Zip(updatedGamesPlayer5.Where(g => g.Season == 2004), (r, g) => r.Matches(g)).All(m => m));
            Assert.IsTrue(gameRowsPlayer5Season2005.Zip(updatedGamesPlayer5.Where(g => g.Season == 2005), (r, g) => r.Matches(g)).All(m => m));
            Assert.AreEqual(syncResult.UpdatedGameRows.OrderBy(r => r.ID).First().RowKey, gameRowsPlayer5Season2004.OrderBy(r => r.ID).First().RowKey);

            syncResult = new SyncGamesResult(gameRowsPlayer5Season2004.Concat(gameRowsPlayer5Season2005), updatedGamesPlayer5);
            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(0, syncResult.UpdatedGameRows.Count);
            Assert.AreEqual(0, syncResult.NewGames.Count);
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

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, syncFoundChanges: false);
            nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("2", nextPlayerFeedRow.Url);
            Assert.IsNull(nextPlayerFeedRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerFeedRow.LastSyncWithChangesTimeUtc);

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, syncFoundChanges: true);
            nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("3", nextPlayerFeedRow.Url);
            Assert.IsNull(nextPlayerFeedRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerFeedRow.LastSyncWithChangesTimeUtc);

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, syncFoundChanges: true);
            nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("1", nextPlayerFeedRow.Url);
            Assert.IsNotNull(nextPlayerFeedRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerFeedRow.LastSyncWithChangesTimeUtc);

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, syncFoundChanges: true);
            nextPlayerFeedRow = (await _syncService.GetNextPlayerFeedRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("2", nextPlayerFeedRow.Url);
            Assert.IsNotNull(nextPlayerFeedRow.LastSyncTimeUtc);
            Assert.IsNotNull(nextPlayerFeedRow.LastSyncWithChangesTimeUtc);

            await _syncService.RequeuePlayerFeedRow(nextPlayerFeedRow, syncFoundChanges: true);
            var nextPlayerFeedRows = await _syncService.GetNextPlayerFeedRows(3, TimeSpan.Zero);
            CollectionAssert.AreEqual(
                new[] { "3", "1", "2" },
                nextPlayerFeedRows.Select(r => r.Url).ToArray());
        }

        [TestMethod]
        public async Task RequeuePlayerRow()
        {
            var playerFeed = new PlayerFeed { Url = "1" };
            var players = Enumerable.Range(1, 3)
                .Select(i => new Player
                {
                    FeedUrl = playerFeed.Url,
                    ID = i.ToString(),
                    FirstSeason = DateTime.UtcNow.Year - 5,
                    LastSeason = DateTime.UtcNow.Year,
                    BirthDate = DateTime.UtcNow.AddYears(-25)
                })
                .ToArray();
            var syncResult = new SyncPlayersResult(Enumerable.Empty<PlayerRow>(), players);
            await _syncService.UpdatePlayersTable(syncResult);

            var nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("1", nextPlayerRow.ID);
            Assert.IsNull(nextPlayerRow.LastSyncSeason);
            Assert.IsNull(nextPlayerRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerRow.LastSyncWithChangesTimeUtc);
            Assert.AreEqual(nextPlayerRow.FirstSeason, nextPlayerRow.GetNextSyncSeason());

            await _syncService.RequeuePlayerRow(nextPlayerRow, nextPlayerRow.GetNextSyncSeason(), syncFoundChanges: false);
            nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("2", nextPlayerRow.ID);
            Assert.IsNull(nextPlayerRow.LastSyncSeason);
            Assert.IsNull(nextPlayerRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerRow.LastSyncWithChangesTimeUtc);
            Assert.AreEqual(nextPlayerRow.FirstSeason, nextPlayerRow.GetNextSyncSeason());

            await _syncService.RequeuePlayerRow(nextPlayerRow, nextPlayerRow.GetNextSyncSeason(), syncFoundChanges: true);
            nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("3", nextPlayerRow.ID);
            Assert.IsNull(nextPlayerRow.LastSyncSeason);
            Assert.IsNull(nextPlayerRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerRow.LastSyncWithChangesTimeUtc);
            Assert.AreEqual(nextPlayerRow.FirstSeason, nextPlayerRow.GetNextSyncSeason());

            await _syncService.RequeuePlayerRow(nextPlayerRow, nextPlayerRow.GetNextSyncSeason(), syncFoundChanges: true);
            nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("1", nextPlayerRow.ID);
            Assert.AreEqual(nextPlayerRow.FirstSeason, nextPlayerRow.LastSyncSeason);
            Assert.IsNotNull(nextPlayerRow.LastSyncTimeUtc);
            Assert.IsNull(nextPlayerRow.LastSyncWithChangesTimeUtc);
            Assert.AreEqual(nextPlayerRow.FirstSeason + 1, nextPlayerRow.GetNextSyncSeason());

            await _syncService.RequeuePlayerRow(nextPlayerRow, nextPlayerRow.GetNextSyncSeason(), syncFoundChanges: true);
            nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("2", nextPlayerRow.ID);
            Assert.AreEqual(nextPlayerRow.FirstSeason, nextPlayerRow.LastSyncSeason);
            Assert.IsNotNull(nextPlayerRow.LastSyncTimeUtc);
            Assert.IsNotNull(nextPlayerRow.LastSyncWithChangesTimeUtc);
            Assert.AreEqual(nextPlayerRow.FirstSeason + 1, nextPlayerRow.GetNextSyncSeason());

            await _syncService.RequeuePlayerRow(nextPlayerRow, nextPlayerRow.GetNextSyncSeason(), syncFoundChanges: true);
            var nextPlayerRows = await _syncService.GetNextPlayerRows(3, TimeSpan.Zero);
            CollectionAssert.AreEqual(
                new[] { "3", "1", "2" },
                nextPlayerRows.Select(r => r.ID).ToArray());

            nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("3", nextPlayerRow.ID);

            nextPlayerRow.FirstSeason = 2000;
            nextPlayerRow.LastSeason = 2010;
            await _syncService.RequeuePlayerRow(nextPlayerRow, 2010, syncFoundChanges: true);
            var playerRows = await _syncService.GetPlayerRows(playerFeed);
            Assert.AreEqual("1", playerRows[0].ID);
            Assert.AreEqual("2", playerRows[1].ID);
            Assert.AreEqual("3", playerRows[2].ID);
            Assert.AreEqual(playerRows[0].FirstSeason + 2, playerRows[0].GetNextSyncSeason());
            Assert.AreEqual(playerRows[1].FirstSeason + 2, playerRows[1].GetNextSyncSeason());
            Assert.AreEqual(playerRows[2].FirstSeason, playerRows[2].GetNextSyncSeason());

            nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("1", nextPlayerRow.ID);

            await _syncService.RequeuePlayerRow(nextPlayerRow, nextPlayerRow.GetNextSyncSeason(), syncFoundChanges: true);
            nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("2", nextPlayerRow.ID);

            await _syncService.RequeuePlayerRow(nextPlayerRow, nextPlayerRow.GetNextSyncSeason(), syncFoundChanges: true);
            nextPlayerRow = (await _syncService.GetNextPlayerRows(1, TimeSpan.Zero)).Single();
            Assert.AreEqual("1", nextPlayerRow.ID); // 3 has been deprioritized.
        }
    }
}
