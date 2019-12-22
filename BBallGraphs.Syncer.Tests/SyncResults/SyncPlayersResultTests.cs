using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Syncer.Rows;
using BBallGraphs.Syncer.SyncResults;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BBallGraphs.Syncer.Tests.SyncResults
{
    [TestClass]
    public class SyncPlayersResultTests
    {
        [TestMethod]
        public void FindsChanges()
        {
            var playerRows = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new PlayerRow { Url = i.Item1.ToString(), Name = i.Item2.ToString() })
                .ToArray();
            var players = new[] { (0, 100), (1, 101), (3, 300), (5, 500), (6, 106) }
                .Select(i => new Player { Url = i.Item1.ToString(), Name = i.Item2.ToString() })
                .ToArray();
            var syncResult = new SyncPlayersResult(playerRows, players);

            CollectionAssert.AreEquivalent(
                playerRows.Where(r => r.Url == "2" || r.Url == "4").ToArray(),
                syncResult.DefunctPlayerRows.ToArray());
            CollectionAssert.AreEquivalent(
                players.Where(p => p.Url == "0" || p.Url == "6").ToArray(),
                syncResult.NewPlayers.ToArray());
            CollectionAssert.AreEquivalent(
                playerRows.Where(p => p.Url == "3" || p.Url == "5").ToArray(),
                syncResult.UpdatedPlayerRows.ToArray());
            CollectionAssert.AreEqual(
                new[] { "1", "2", "3", "4", "5" }, playerRows.Select(r => r.Url).ToArray());
            CollectionAssert.AreEqual(
                new[] { "101", "102", "300", "104", "500" }, playerRows.Select(r => r.Name).ToArray());
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void DoesntFindChanges()
        {
            var playerRows = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new PlayerRow { Url = i.Item1.ToString(), Name = i.Item2.ToString() })
                .ToArray();
            var players = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new Player { Url = i.Item1.ToString(), Name = i.Item2.ToString() })
                .ToArray();
            var syncResult = new SyncPlayersResult(playerRows, players);

            Assert.AreEqual(0, syncResult.DefunctPlayerRows.Count);
            Assert.AreEqual(0, syncResult.NewPlayers.Count);
            Assert.AreEqual(0, syncResult.UpdatedPlayerRows.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllDefunct()
        {
            var playerRows = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new PlayerRow { Url = i.Item1.ToString(), Name = i.Item2.ToString() })
                .ToArray();
            var syncResult = new SyncPlayersResult(playerRows, Enumerable.Empty<Player>());

            CollectionAssert.AreEqual(playerRows, syncResult.DefunctPlayerRows.ToArray());
            Assert.AreEqual(0, syncResult.NewPlayers.Count);
            Assert.AreEqual(0, syncResult.UpdatedPlayerRows.Count);
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllNew()
        {
            var players = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new Player { Url = i.Item1.ToString(), Name = i.Item2.ToString() })
                .ToArray();
            var syncResult = new SyncPlayersResult(Enumerable.Empty<PlayerRow>(), players);

            CollectionAssert.AreEqual(players, syncResult.NewPlayers.ToArray());
            Assert.AreEqual(0, syncResult.DefunctPlayerRows.Count);
            Assert.AreEqual(0, syncResult.UpdatedPlayerRows.Count);
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllUpdated()
        {
            var playerRows = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new PlayerRow { Url = i.Item1.ToString(), Name = i.Item2.ToString() })
                .ToArray();
            var players = new[] { (1, 1001), (2, 1002), (3, 1003), (4, 1004), (5, 1005) }
                .Select(i => new Player { Url = i.Item1.ToString(), Name = i.Item2.ToString() })
                .ToArray();
            var syncResult = new SyncPlayersResult(playerRows, players);

            Assert.AreEqual(0, syncResult.DefunctPlayerRows.Count);
            Assert.AreEqual(0, syncResult.NewPlayers.Count);
            CollectionAssert.AreEquivalent(playerRows, syncResult.UpdatedPlayerRows.ToArray());
            CollectionAssert.AreEqual(
                new[] { "1", "2", "3", "4", "5" }, playerRows.Select(r => r.Url).ToArray());
            CollectionAssert.AreEqual(
                new[] { "1001", "1002", "1003", "1004", "1005" }, playerRows.Select(r => r.Name).ToArray());
            Assert.IsTrue(playerRows.Zip(players, (r, p) => r.Matches(p)).All(m => m));
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void DoesntFindChangesWhenAllEmpty()
        {
            var syncResult = new SyncPlayersResult(
                Enumerable.Empty<PlayerRow>(), Enumerable.Empty<Player>());

            Assert.AreEqual(0, syncResult.DefunctPlayerRows.Count);
            Assert.AreEqual(0, syncResult.NewPlayers.Count);
            Assert.AreEqual(0, syncResult.UpdatedPlayerRows.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }
    }
}
