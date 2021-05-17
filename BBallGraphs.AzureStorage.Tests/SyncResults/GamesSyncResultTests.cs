using BBallGraphs.AzureStorage.Rows;
using BBallGraphs.AzureStorage.SyncResults;
using BBallGraphs.BasketballReferenceScraper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BBallGraphs.AzureStorage.Tests.SyncResults
{
    [TestClass]
    public class GamesSyncResultTests
    {
        [TestMethod]
        public void FindsChanges()
        {
            var gameRows = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new GameRow { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var games = new[] { (0, 100), (1, 101), (3, 300), (5, 500), (6, 106) }
                .Select(i => new Game { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var syncResult = new GamesSyncResult(gameRows, games);

            CollectionAssert.AreEquivalent(
                gameRows.Where(r => r.ID == "2" || r.ID == "4").ToArray(),
                syncResult.DefunctGameRows.ToArray());
            Assert.IsTrue(games.Where(g => g.ID == "0" || g.ID == "6")
                .Zip(syncResult.NewGameRows).All(p => p.First.Matches(p.Second)));
            CollectionAssert.AreEquivalent(
                gameRows.Where(r => r.ID == "3" || r.ID == "5").ToArray(),
                syncResult.UpdatedGameRows.ToArray());
            CollectionAssert.AreEqual(
                new[] { "1", "2", "3", "4", "5" }, gameRows.Select(r => r.ID).ToArray());
            CollectionAssert.AreEqual(
                new[] { 101, 102, 300, 104, 500 }, gameRows.Select(r => r.Points).ToArray());
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void DoesntFindChanges()
        {
            var gameRows = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new GameRow { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var games = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new Game { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var syncResult = new GamesSyncResult(gameRows, games);

            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(0, syncResult.NewGameRows.Count);
            Assert.AreEqual(0, syncResult.UpdatedGameRows.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllDefunct()
        {
            var gameRows = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new GameRow { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var syncResult = new GamesSyncResult(gameRows, Enumerable.Empty<Game>());

            CollectionAssert.AreEqual(gameRows, syncResult.DefunctGameRows.ToArray());
            Assert.AreEqual(0, syncResult.NewGameRows.Count);
            Assert.AreEqual(0, syncResult.UpdatedGameRows.Count);
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllNew()
        {
            var games = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new Game { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var syncResult = new GamesSyncResult(Enumerable.Empty<GameRow>(), games);

            Assert.IsTrue(games.Zip(syncResult.NewGameRows).All(p => p.First.Matches(p.Second)));
            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(0, syncResult.UpdatedGameRows.Count);
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllUpdated()
        {
            var gameRows = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new GameRow { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var games = new[] { (1, 1001), (2, 1002), (3, 1003), (4, 1004), (5, 1005) }
                .Select(i => new Game { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var syncResult = new GamesSyncResult(gameRows, games);

            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(0, syncResult.NewGameRows.Count);
            CollectionAssert.AreEquivalent(gameRows, syncResult.UpdatedGameRows.ToArray());
            CollectionAssert.AreEqual(
                new[] { "1", "2", "3", "4", "5" }, gameRows.Select(r => r.ID).ToArray());
            CollectionAssert.AreEqual(
                new[] { 1001, 1002, 1003, 1004, 1005 }, gameRows.Select(r => r.Points).ToArray());
            Assert.IsTrue(gameRows.Zip(games, (r, g) => r.Matches(g)).All(m => m));
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void DoesntFindChangesWhenAllEmpty()
        {
            var syncResult = new GamesSyncResult(
                Enumerable.Empty<GameRow>(), Enumerable.Empty<Game>());

            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(0, syncResult.NewGameRows.Count);
            Assert.AreEqual(0, syncResult.UpdatedGameRows.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }
    }
}
