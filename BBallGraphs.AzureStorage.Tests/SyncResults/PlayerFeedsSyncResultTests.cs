using BBallGraphs.AzureStorage.Rows;
using BBallGraphs.AzureStorage.SyncResults;
using BBallGraphs.BasketballReferenceScraper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BBallGraphs.AzureStorage.Tests.SyncResults
{
    [TestClass]
    public class PlayerFeedsSyncResultTests
    {
        [TestMethod]
        public void FindsChanges()
        {
            var playerFeedRows = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                .Select(i => new PlayerFeedRow { Url = i.ToString() })
                .ToArray();
            var playerFeeds = new[] { 0, 1, 2, 3, 6, 7, 8, 10, 11 }
                .Select(i => new PlayerFeed { Url = i.ToString() })
                .ToArray();
            var syncResult = new PlayerFeedsSyncResult(playerFeedRows, playerFeeds);

            CollectionAssert.AreEquivalent(
                playerFeedRows.Where(r => r.Url == "4" || r.Url == "5" || r.Url == "9").ToArray(),
                syncResult.DefunctPlayerFeedRows.ToArray());
            Assert.IsTrue(playerFeeds.Where(f => f.Url == "0" || f.Url == "10" || f.Url == "11")
                .Zip(syncResult.NewPlayerFeedRows).All(p => p.First.Matches(p.Second)));
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void DoesntFindChanges()
        {
            var playerFeedRows = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                .Select(i => new PlayerFeedRow { Url = i.ToString() })
                .ToArray();
            var playerFeeds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                .Select(i => new PlayerFeed { Url = i.ToString() })
                .ToArray();
            var syncResult = new PlayerFeedsSyncResult(playerFeedRows, playerFeeds);

            Assert.AreEqual(0, syncResult.DefunctPlayerFeedRows.Count);
            Assert.AreEqual(0, syncResult.NewPlayerFeedRows.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllDefunct()
        {
            var playerFeedRows = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                .Select(i => new PlayerFeedRow { Url = i.ToString() })
                .ToArray();
            var syncResult = new PlayerFeedsSyncResult(playerFeedRows, Enumerable.Empty<PlayerFeed>());

            CollectionAssert.AreEqual(playerFeedRows, syncResult.DefunctPlayerFeedRows.ToArray());
            Assert.AreEqual(0, syncResult.NewPlayerFeedRows.Count);
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllNew()
        {
            var playerFeeds = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
                .Select(i => new PlayerFeed { Url = i.ToString() })
                .ToArray();
            var syncResult = new PlayerFeedsSyncResult(Enumerable.Empty<PlayerFeedRow>(), playerFeeds);

            Assert.IsTrue(playerFeeds.Zip(syncResult.NewPlayerFeedRows).All(p => p.First.Matches(p.Second)));
            Assert.AreEqual(0, syncResult.DefunctPlayerFeedRows.Count);
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void DoesntFindChangesWhenAllEmpty()
        {
            var syncResult = new PlayerFeedsSyncResult(
                Enumerable.Empty<PlayerFeedRow>(), Enumerable.Empty<PlayerFeed>());

            Assert.AreEqual(0, syncResult.DefunctPlayerFeedRows.Count);
            Assert.AreEqual(0, syncResult.NewPlayerFeedRows.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }
    }
}
