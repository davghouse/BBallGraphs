using BBallGraphs.Scrapers.BasketballReference;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.Syncer.Rows
{
    public class PlayerFeedRow : TableEntity, IPlayerFeed
    {
        public string Url { get; set; }

        public DateTime? LastSyncTimeUtc { get; set; }
        public DateTime? LastSyncWithChangesTimeUtc { get; set; }

        public static PlayerFeedRow CreateRow(IPlayerFeed playerFeed, DateTime rowKeyTimeUtc)
        {
            var playerFeedRow = new PlayerFeedRow
            {
                PartitionKey = "0",
                RowKey = GetRowKey(rowKeyTimeUtc),
            };
            playerFeed.CopyTo(playerFeedRow);

            return playerFeedRow;
        }

        public static IEnumerable<PlayerFeedRow> CreateRows(IEnumerable<IPlayerFeed> playerFeeds)
        {
            var utcNow = DateTime.UtcNow;
            int rowKeyDeduplicator = 0;

            return playerFeeds
                .Select(f => CreateRow(f, utcNow.AddTicks(rowKeyDeduplicator++)));
        }

        public PlayerFeedRow CreateRequeuedRow(DateTime syncTimeUtc, bool syncFoundChanges)
        {
            var requeuedPlayerFeedRow = CreateRow(this, syncTimeUtc);
            requeuedPlayerFeedRow.LastSyncTimeUtc = syncTimeUtc;
            requeuedPlayerFeedRow.LastSyncWithChangesTimeUtc = syncFoundChanges ? syncTimeUtc : LastSyncWithChangesTimeUtc;

            return requeuedPlayerFeedRow;
        }

        public static string GetRowKey(DateTime rowKeyTimeUtc)
            => rowKeyTimeUtc.Ticks.ToString("D19");

        public override string ToString()
            => Url;
    }
}
