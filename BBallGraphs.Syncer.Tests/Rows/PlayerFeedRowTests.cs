using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Syncer.Rows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace BBallGraphs.Syncer.Tests.Rows
{
    [TestClass]
    public class PlayerFeedRowTests
    {
        [TestMethod]
        public void CreateRow()
        {
            var playerFeed = new PlayerFeed { Url = "a" };
            var playerFeedRow = PlayerFeedRow.CreateRow(playerFeed, DateTime.UtcNow);

            Assert.AreEqual("0", playerFeedRow.PartitionKey);
            Assert.AreEqual("a", playerFeedRow.Url);
            Assert.IsNull(playerFeedRow.LastSyncTimeUtc);
            Assert.IsNull(playerFeedRow.LastSyncWithChangesTimeUtc);
        }

        [TestMethod]
        public void CreateRows()
        {
            var playerFeeds = Enumerable.Range(0, 26)
                .Select(i => new PlayerFeed { Url = ((char)('a' + i)).ToString() });
            var playerFeedRows = PlayerFeedRow.CreateRows(playerFeeds).ToArray();

            Assert.AreEqual(26, playerFeedRows.Length);
            CollectionAssert.AreEqual(
                playerFeeds.Select(f => f.Url).ToArray(),
                playerFeedRows.Select(r => r.Url).ToArray());
            Assert.IsTrue(playerFeedRows.All(r => r.PartitionKey == "0"));
            Assert.AreEqual(playerFeedRows.Length, playerFeedRows.Select(r => r.RowKey).Distinct().Count());
        }

        [TestMethod]
        public void RowKeysMaintainOrder()
        {
            var dateTimesOrderedByTime = Enumerable.Range(0, 100)
                .Concat(Enumerable.Range(50, 100))
                .Select(i => DateTime.UtcNow.AddMinutes(i))
                .OrderBy(d => d)
                .ToArray();
            var dateTimesOrderedByRowKey = dateTimesOrderedByTime
                .OrderBy(PlayerFeedRow.GetRowKey)
                .ToArray();

            CollectionAssert.AreEqual(dateTimesOrderedByTime, dateTimesOrderedByRowKey);
        }

        [TestMethod]
        public void CreateRequeuedRow()
        {
            var rowKeyTimeUtc = DateTime.UtcNow;
            var requeuedRowKeyTimeUtc = DateTime.UtcNow.AddTicks(1);
            var playerFeed = new PlayerFeed { Url = "a" };
            var playerFeedRow = PlayerFeedRow.CreateRow(playerFeed, rowKeyTimeUtc);
            var requeuedPlayerFeedRow = playerFeedRow.CreateRequeuedRow(
                syncTimeUtc: requeuedRowKeyTimeUtc,
                syncFoundChanges: true);

            Assert.AreEqual(requeuedRowKeyTimeUtc, requeuedPlayerFeedRow.LastSyncTimeUtc);
            Assert.AreEqual(requeuedRowKeyTimeUtc, requeuedPlayerFeedRow.LastSyncWithChangesTimeUtc);

            var reRequeuedRowKeyTimeUtc = DateTime.UtcNow.AddTicks(2);
            var reRequeuedPlayerFeedRow = requeuedPlayerFeedRow.CreateRequeuedRow(
                syncTimeUtc: reRequeuedRowKeyTimeUtc,
                syncFoundChanges: false);

            Assert.AreEqual(reRequeuedRowKeyTimeUtc, reRequeuedPlayerFeedRow.LastSyncTimeUtc);
            Assert.AreEqual(requeuedRowKeyTimeUtc, reRequeuedPlayerFeedRow.LastSyncWithChangesTimeUtc);
        }
    }
}
