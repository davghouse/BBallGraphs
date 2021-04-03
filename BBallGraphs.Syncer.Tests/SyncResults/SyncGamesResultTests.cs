using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Syncer.Rows;
using BBallGraphs.Syncer.SyncResults;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BBallGraphs.Syncer.Tests.SyncResults
{
    [TestClass]
    public class SyncGamesResultTests
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
            var syncResult = new SyncGamesResult(gameRows, games);

            CollectionAssert.AreEquivalent(
                gameRows.Where(r => r.ID == "2" || r.ID == "4").ToArray(),
                syncResult.DefunctGameRows.ToArray());
            CollectionAssert.AreEquivalent(
                games.Where(g => g.ID == "0" || g.ID == "6").ToArray(),
                syncResult.NewGames.ToArray());
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
            var syncResult = new SyncGamesResult(gameRows, games);

            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(0, syncResult.NewGames.Count);
            Assert.AreEqual(0, syncResult.UpdatedGameRows.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllDefunct()
        {
            var gameRows = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new GameRow { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var syncResult = new SyncGamesResult(gameRows, Enumerable.Empty<Game>());

            CollectionAssert.AreEqual(gameRows, syncResult.DefunctGameRows.ToArray());
            Assert.AreEqual(0, syncResult.NewGames.Count);
            Assert.AreEqual(0, syncResult.UpdatedGameRows.Count);
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllNew()
        {
            var games = new[] { (1, 101), (2, 102), (3, 103), (4, 104), (5, 105) }
                .Select(i => new Game { ID = i.Item1.ToString(), Points = i.Item2 })
                .ToArray();
            var syncResult = new SyncGamesResult(Enumerable.Empty<GameRow>(), games);

            CollectionAssert.AreEqual(games, syncResult.NewGames.ToArray());
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
            var syncResult = new SyncGamesResult(gameRows, games);

            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(0, syncResult.NewGames.Count);
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
            var syncResult = new SyncGamesResult(
                Enumerable.Empty<GameRow>(), Enumerable.Empty<Game>());

            Assert.AreEqual(0, syncResult.DefunctGameRows.Count);
            Assert.AreEqual(0, syncResult.NewGames.Count);
            Assert.AreEqual(0, syncResult.UpdatedGameRows.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }
    }
}
