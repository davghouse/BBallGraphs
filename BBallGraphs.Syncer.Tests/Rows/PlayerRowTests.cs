using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Scrapers.Helpers;
using BBallGraphs.Syncer.Rows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace BBallGraphs.Syncer.Tests.Rows
{
    [TestClass]
    public class PlayerRowTests
    {
        [TestMethod]
        public void CreateRow()
        {
            var player = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                ID = "jamesle01",
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
            Assert.AreEqual("https://www.basketball-reference.com/players/j/", playerRow.FeedUrl);
            Assert.AreEqual("jamesle01", playerRow.ID);
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
    }
}
