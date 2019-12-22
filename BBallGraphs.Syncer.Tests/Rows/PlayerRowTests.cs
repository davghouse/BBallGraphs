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
                Url = "https://www.basketball-reference.com/players/j/jamesle01.html",
                Name = "LeBron James",
                FromYear = 2004,
                ToYear = 2020,
                Position = "F-G",
                HeightInches = 81,
                WeightPounds = 250,
                BirthDate = new DateTime(1984, 12, 30).AsUtc(),
                FeedUrl = "https://www.basketball-reference.com/players/j/"
            };
            var playerRow = PlayerRow.CreateRow(player, DateTime.UtcNow);

            Assert.AreEqual("0", playerRow.PartitionKey);
            Assert.AreEqual("https://www.basketball-reference.com/players/j/jamesle01.html", playerRow.Url);
            Assert.AreEqual("LeBron James", playerRow.Name);
            Assert.AreEqual(2004, playerRow.FromYear);
            Assert.AreEqual(2020, playerRow.ToYear);
            Assert.AreEqual("F-G", playerRow.Position);
            Assert.AreEqual(81, playerRow.HeightInches);
            Assert.AreEqual(250, playerRow.WeightPounds);
            Assert.AreEqual(new DateTime(1984, 12, 30).AsUtc(), playerRow.BirthDate);
            Assert.AreEqual("https://www.basketball-reference.com/players/j/", playerRow.FeedUrl);
        }

        [TestMethod]
        public void CreateRows()
        {
            var players = Enumerable.Range(0, 50)
                .Select(i => new Player { Url = i.ToString(), Name = $"player {i}" });
            var playerRows = PlayerRow.CreateRows(players).ToArray();

            Assert.AreEqual(50, playerRows.Length);
            CollectionAssert.AreEqual(
                players.Select(f => f.Url).ToArray(),
                playerRows.Select(r => r.Url).ToArray());
            Assert.IsTrue(playerRows.All(r => r.PartitionKey == "0"));
            Assert.AreEqual(playerRows.Length, playerRows.Select(r => r.RowKey).Distinct().Count());
        }
    }
}
