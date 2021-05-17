using BBallGraphs.AzureStorage.Rows;
using BBallGraphs.BasketballReferenceScraper;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.AzureStorage.SyncResults
{
    public class PlayerFeedsSyncResult
    {
        public PlayerFeedsSyncResult(
            IEnumerable<PlayerFeedRow> playerFeedRows,
            IEnumerable<PlayerFeed> playerFeeds)
        {
            var playerFeedRowsByID = playerFeedRows.ToDictionary(r => r.Url);
            var playerFeedsByID = playerFeeds.ToDictionary(f => f.Url);

            DefunctPlayerFeedRows = playerFeedRowsByID.Values
                .Where(r => !playerFeedsByID.ContainsKey(r.Url))
                .ToArray();
            NewPlayerFeedRows = PlayerFeedRow.CreateRows(playerFeedsByID.Values
                .Where(f => !playerFeedRowsByID.ContainsKey(f.Url)))
                .ToArray();
        }

        public IReadOnlyList<PlayerFeedRow> DefunctPlayerFeedRows { get; }
        public IReadOnlyList<PlayerFeedRow> NewPlayerFeedRows { get; }

        public bool FoundChanges
            => DefunctPlayerFeedRows.Any() || NewPlayerFeedRows.Any();
    }
}
