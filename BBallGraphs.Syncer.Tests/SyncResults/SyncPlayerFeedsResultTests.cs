using BBallGraphs.BasketballReferenceScraper;
using BBallGraphs.Syncer.Rows;
using BBallGraphs.Syncer.SyncResults;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BBallGraphs.Syncer.Tests.SyncResults
{
    [TestClass]
    public class SyncPlayerFeedsResultTests
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
            var syncResult = new SyncPlayerFeedsResult(playerFeedRows, playerFeeds);

            CollectionAssert.AreEquivalent(
                playerFeedRows.Where(r => r.Url == "4" || r.Url == "5" || r.Url == "9").ToArray(),
                syncResult.DefunctPlayerFeedRows.ToArray());
            CollectionAssert.AreEquivalent(
                playerFeeds.Where(f => f.Url == "0" || f.Url == "10" || f.Url == "11").ToArray(),
                syncResult.NewPlayerFeeds.ToArray());
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
            var syncResult = new SyncPlayerFeedsResult(playerFeedRows, playerFeeds);

            Assert.AreEqual(0, syncResult.DefunctPlayerFeedRows.Count);
            Assert.AreEqual(0, syncResult.NewPlayerFeeds.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllDefunct()
        {
            var playerFeedRows = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                .Select(i => new PlayerFeedRow { Url = i.ToString() })
                .ToArray();
            var syncResult = new SyncPlayerFeedsResult(playerFeedRows, Enumerable.Empty<PlayerFeed>());

            CollectionAssert.AreEqual(playerFeedRows, syncResult.DefunctPlayerFeedRows.ToArray());
            Assert.AreEqual(0, syncResult.NewPlayerFeeds.Count);
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void FindsChangesWhenAllNew()
        {
            var playerFeeds = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
                .Select(i => new PlayerFeed { Url = i.ToString() })
                .ToArray();
            var syncResult = new SyncPlayerFeedsResult(Enumerable.Empty<PlayerFeedRow>(), playerFeeds);

            CollectionAssert.AreEqual(playerFeeds, syncResult.NewPlayerFeeds.ToArray());
            Assert.AreEqual(0, syncResult.DefunctPlayerFeedRows.Count);
            Assert.IsTrue(syncResult.FoundChanges);
        }

        [TestMethod]
        public void DoesntFindChangesWhenAllEmpty()
        {
            var syncResult = new SyncPlayerFeedsResult(
                Enumerable.Empty<PlayerFeedRow>(), Enumerable.Empty<PlayerFeed>());

            Assert.AreEqual(0, syncResult.DefunctPlayerFeedRows.Count);
            Assert.AreEqual(0, syncResult.NewPlayerFeeds.Count);
            Assert.IsFalse(syncResult.FoundChanges);
        }
    }
}
