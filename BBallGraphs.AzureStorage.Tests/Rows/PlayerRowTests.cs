using BBallGraphs.AzureStorage.Rows;
using BBallGraphs.BasketballReferenceScraper;
using BBallGraphs.BasketballReferenceScraper.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace BBallGraphs.AzureStorage.Tests.Rows
{
    [TestClass]
    public class PlayerRowTests
    {
        [TestMethod]
        public void CreateRow()
        {
            var player = new Player
            {
                ID = "jamesle01",
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                Name = "LeBron James",
                FirstSeason = 2004,
                LastSeason = 2020,
                Position = "F-G",
                HeightInInches = 81,
                WeightInPounds = 250,
                BirthDate = new DateTime(1984, 12, 30).AsUtc(),
            };
            var playerRow = PlayerRow.CreateRow(player, DateTime.UtcNow);

            Assert.AreEqual("0", playerRow.PartitionKey);
            Assert.AreEqual("jamesle01", playerRow.ID);
            Assert.AreEqual("https://www.basketball-reference.com/players/j/", playerRow.FeedUrl);
            Assert.AreEqual("https://www.basketball-reference.com/players/j/jamesle01.html", playerRow.GetProfileUrl());
            Assert.AreEqual("LeBron James", playerRow.Name);
            Assert.AreEqual(2004, playerRow.FirstSeason);
            Assert.AreEqual(2020, playerRow.LastSeason);
            Assert.AreEqual("F-G", playerRow.Position);
            Assert.AreEqual(81, playerRow.HeightInInches);
            Assert.AreEqual(250, playerRow.WeightInPounds);
            Assert.AreEqual(new DateTime(1984, 12, 30).AsUtc(), playerRow.BirthDate);
        }

        [TestMethod]
        public void CreateRows()
        {
            var players = Enumerable.Range(0, 50)
                .Select(i => new Player { ID = i.ToString(), Name = $"player {i}" });
            var playerRows = PlayerRow.CreateRows(players).ToArray();

            Assert.AreEqual(50, playerRows.Length);
            CollectionAssert.AreEqual(
                players.Select(f => f.ID).ToArray(),
                playerRows.Select(r => r.ID).ToArray());
            Assert.IsTrue(playerRows.All(r => r.PartitionKey == "0"));
            Assert.AreEqual(playerRows.Length, playerRows.Select(r => r.RowKey).Distinct().Count());
        }

        [TestMethod]
        public void GetNextSyncSeasonForActivePlayer()
        {
            var utcNow = DateTime.UtcNow;
            var playerRow = new PlayerRow
            {
                ID = "testte01",
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                Name = "test test",
                FirstSeason = utcNow.Year - 10,
                LastSeason = utcNow.Year,
            };
            Assert.AreEqual(playerRow.FirstSeason, playerRow.GetNextSyncSeason());

            playerRow.LastSyncSeason = playerRow.FirstSeason;
            Assert.AreEqual(playerRow.FirstSeason + 1, playerRow.GetNextSyncSeason());

            playerRow.LastSyncSeason = playerRow.LastSeason;
            Assert.AreEqual(playerRow.LastSeason, playerRow.GetNextSyncSeason());
        }

        [TestMethod]
        public void GetNextSyncSeasonForRetiredPlayer()
        {
            var playerRow = new PlayerRow
            {
                ID = "testte01",
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                Name = "test test",
                FirstSeason = 2000,
                LastSeason = 2010,
            };
            Assert.AreEqual(playerRow.FirstSeason, playerRow.GetNextSyncSeason());

            playerRow.LastSyncSeason = playerRow.FirstSeason;
            Assert.AreEqual(playerRow.FirstSeason + 1, playerRow.GetNextSyncSeason());

            playerRow.LastSyncSeason = playerRow.LastSeason;
            Assert.AreEqual(playerRow.FirstSeason, playerRow.GetNextSyncSeason());
        }

        [TestMethod]
        public void CreateRequeuedRowForActivePlayer()
        {
            var utcNow = DateTime.UtcNow;
            var player = new Player
            {
                ID = "testte01",
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                Name = "test test",
                FirstSeason = utcNow.Year - 10,
                LastSeason = utcNow.Year,
            };
            var playerRow = PlayerRow.CreateRow(player, utcNow);
            Assert.AreEqual(playerRow.FirstSeason, playerRow.GetNextSyncSeason());
            Assert.AreEqual(null, playerRow.LastSyncSeason);
            Assert.AreEqual(null, playerRow.LastSyncTimeUtc);
            Assert.AreEqual(null, playerRow.LastSyncWithChangesTimeUtc);
            Assert.AreEqual(PlayerRow.GetRowKey(utcNow), playerRow.RowKey);

            for (int i = 0; i < 10; ++i)
            {
                playerRow = playerRow.CreateRequeuedRow(utcNow.AddTicks(i), playerRow.FirstSeason + i, syncFoundChanges: true);
                Assert.AreEqual(playerRow.FirstSeason + i + 1, playerRow.GetNextSyncSeason());
                Assert.AreEqual(playerRow.FirstSeason + i, playerRow.LastSyncSeason);
                Assert.AreEqual(utcNow.AddTicks(i), playerRow.LastSyncTimeUtc);
                Assert.AreEqual(utcNow.AddTicks(i), playerRow.LastSyncWithChangesTimeUtc);
                Assert.AreEqual(PlayerRow.GetRowKey(utcNow.AddTicks(i)), playerRow.RowKey);
            }

            playerRow = playerRow.CreateRequeuedRow(utcNow.AddTicks(10), playerRow.FirstSeason + 10, syncFoundChanges: true);
            Assert.AreEqual(playerRow.LastSeason, playerRow.GetNextSyncSeason());
            Assert.AreEqual(playerRow.LastSeason, playerRow.LastSyncSeason);
            Assert.AreEqual(utcNow.AddTicks(10), playerRow.LastSyncTimeUtc);
            Assert.AreEqual(utcNow.AddTicks(10), playerRow.LastSyncWithChangesTimeUtc);
            Assert.AreEqual(PlayerRow.GetRowKey(utcNow.AddTicks(10)), playerRow.RowKey);
        }

        [TestMethod]
        public void CreateRequeuedRowForRetiredPlayer()
        {
            var utcNow = DateTime.UtcNow;
            var player = new Player
            {
                ID = "testte01",
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                Name = "test test",
                FirstSeason = 2000,
                LastSeason = 2010,
            };
            var playerRow = PlayerRow.CreateRow(player, utcNow);
            Assert.AreEqual(playerRow.FirstSeason, playerRow.GetNextSyncSeason());
            Assert.AreEqual(null, playerRow.LastSyncSeason);
            Assert.AreEqual(null, playerRow.LastSyncTimeUtc);
            Assert.AreEqual(null, playerRow.LastSyncWithChangesTimeUtc);
            Assert.AreEqual(PlayerRow.GetRowKey(utcNow), playerRow.RowKey);

            for (int i = 0; i < 10; ++i)
            {
                playerRow = playerRow.CreateRequeuedRow(utcNow.AddTicks(i), playerRow.FirstSeason + i, syncFoundChanges: true);
                Assert.AreEqual(playerRow.FirstSeason + i + 1, playerRow.GetNextSyncSeason());
                Assert.AreEqual(playerRow.FirstSeason + i, playerRow.LastSyncSeason);
                Assert.AreEqual(utcNow.AddTicks(i), playerRow.LastSyncTimeUtc);
                Assert.AreEqual(utcNow.AddTicks(i), playerRow.LastSyncWithChangesTimeUtc);
                Assert.AreEqual(PlayerRow.GetRowKey(utcNow.AddTicks(i)), playerRow.RowKey);
            }

            playerRow = playerRow.CreateRequeuedRow(utcNow.AddTicks(10), playerRow.FirstSeason + 10, syncFoundChanges: true);
            Assert.AreEqual(playerRow.FirstSeason, playerRow.GetNextSyncSeason());
            Assert.AreEqual(playerRow.LastSeason, playerRow.LastSyncSeason);
            Assert.AreEqual(utcNow.AddTicks(10), playerRow.LastSyncTimeUtc);
            Assert.AreEqual(utcNow.AddTicks(10), playerRow.LastSyncWithChangesTimeUtc);
            // Deprioritized due to being retired.
            Assert.AreEqual(PlayerRow.GetRowKey(utcNow.AddTicks(10).AddDays(180)), playerRow.RowKey);
        }
    }
}
