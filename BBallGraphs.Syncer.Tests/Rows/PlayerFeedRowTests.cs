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
        public void CreateRows()
        {
            var playerFeeds = Enumerable.Range(0, 26)
                .Select(i => new PlayerFeed(((char)('a' + i)).ToString()));
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
    }
}
