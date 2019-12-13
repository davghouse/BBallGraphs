using BBallGraphs.Scrapers.BasketballReference;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace BBallGraphs.Syncer.Rows
{
    public class PlayerFeedRow : TableEntity
    {
        public string Url { get; set; }
        public DateTime? LastSyncedTimeUtc { get; set; }

        public static IEnumerable<PlayerFeedRow> CreateRows(IEnumerable<PlayerFeed> playerFeeds)
        {
            DateTime utcNow = DateTime.UtcNow;
            int rowKeyDeduplicator = 0;

            foreach (var playerFeed in playerFeeds)
                yield return new PlayerFeedRow
                {
                    PartitionKey = "0",
                    RowKey = GetRowKey(utcNow.AddTicks(rowKeyDeduplicator++)),

                    Url = playerFeed.Url
                };
        }

        public static string GetRowKey(DateTime dateTimeUtc)
            => dateTimeUtc.Ticks.ToString("D19");
    }
}
