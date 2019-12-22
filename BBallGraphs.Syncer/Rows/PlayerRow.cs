using BBallGraphs.Scrapers.BasketballReference;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.Syncer.Rows
{
    public class PlayerRow : TableEntity, IPlayer
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public int FromYear { get; set; }
        public int ToYear { get; set; }
        public string Position { get; set; }
        public double HeightInches { get; set; }
        public double? WeightPounds { get; set; }
        public DateTime BirthDate { get; set; }
        public string FeedUrl { get; set; }

        public static PlayerRow CreateRow(IPlayer player, DateTime rowKeyTimeUtc)
        {
            var playerRow = new PlayerRow
            {
                PartitionKey = "0",
                RowKey = GetRowKey(rowKeyTimeUtc),
            };
            player.CopyTo(playerRow);

            return playerRow;
        }

        public static IEnumerable<PlayerRow> CreateRows(IEnumerable<IPlayer> players)
        {
            DateTime utcNow = DateTime.UtcNow;
            int rowKeyDeduplicator = 0;

            return players
                .Select(p => CreateRow(p, utcNow.AddTicks(rowKeyDeduplicator++)));
        }

        public static string GetRowKey(DateTime rowKeyTimeUtc)
            => rowKeyTimeUtc.Ticks.ToString("D19");

        public override string ToString()
            => $"{Name} ({FromYear} - {ToYear})";
    }
}
