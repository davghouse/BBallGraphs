using BBallGraphs.BasketballReferenceScraper;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.AzureStorage.Rows
{
    public class PlayerRow : TableEntity, IPlayer
    {
        public string ID { get; set; }
        public string FeedUrl { get; set; }
        public string Name { get; set; }
        public int FirstSeason { get; set; }
        public int LastSeason { get; set; }
        public string Position { get; set; }
        public double? HeightInInches { get; set; }
        public double? WeightInPounds { get; set; }
        public DateTime BirthDate { get; set; }

        public int? LastSyncSeason { get; set; }
        public DateTime? LastSyncTimeUtc { get; set; }
        public DateTime? LastSyncWithChangesTimeUtc { get; set; }

        public bool HasSyncedGames => LastSyncWithChangesTimeUtc.HasValue;

        public int GetNextSyncSeason()
            => !LastSyncSeason.HasValue ? FirstSeason
            : LastSyncSeason < LastSeason ? LastSyncSeason.Value + 1
            // When LastSyncSeason == LastSeason:
            : this.IsProbablyRetired() ? FirstSeason // Circle back to pick up any data updates or typo fixes.
            : LastSeason; // Keep trying the last season to pick up any new games.

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
            var utcNow = DateTime.UtcNow;
            int rowKeyDeduplicator = 0;

            return players
                .Select(p => CreateRow(p, utcNow.AddTicks(rowKeyDeduplicator++)));
        }

        public PlayerRow CreateRequeuedRow(DateTime syncTimeUtc, int syncSeason, bool syncFoundChanges)
        {
            // There are a lot of players. To make sure active players get their stats synced promptly,
            // it's necessary to sync probably retired players less frequently. Deprioritize retired
            // players more and more the longer they've gone without a changing sync.
            var lastSyncWithChangesTimeUtc = syncFoundChanges ? syncTimeUtc : LastSyncWithChangesTimeUtc;
            var prioritizedRowKeyTimeUtc = syncSeason == LastSeason && this.IsProbablyRetired()
                ? syncTimeUtc.AddDays((syncTimeUtc - (lastSyncWithChangesTimeUtc ?? syncTimeUtc)).TotalDays / 4 + 180)
                : syncTimeUtc;

            var requeuedPlayerRow = CreateRow(this, prioritizedRowKeyTimeUtc);
            requeuedPlayerRow.LastSyncSeason = syncSeason;
            requeuedPlayerRow.LastSyncTimeUtc = syncTimeUtc;
            requeuedPlayerRow.LastSyncWithChangesTimeUtc = lastSyncWithChangesTimeUtc;

            return requeuedPlayerRow;
        }

        public static string GetRowKey(DateTime rowKeyTimeUtc)
            => rowKeyTimeUtc.Ticks.ToString("D19");

        public override string ToString()
            => $"{Name} ({FirstSeason} - {LastSeason})";
    }
}
