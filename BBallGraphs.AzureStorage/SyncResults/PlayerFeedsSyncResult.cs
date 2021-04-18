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
            var playerFeedRowsDict = playerFeedRows.ToDictionary(r => r.Url);
            var playerFeedsDict = playerFeeds.ToDictionary(f => f.Url);

            DefunctPlayerFeedRows = playerFeedRowsDict.Values
                .Where(r => !playerFeedsDict.ContainsKey(r.Url))
                .ToArray();
            NewPlayerFeeds = playerFeedsDict.Values
                .Where(f => !playerFeedRowsDict.ContainsKey(f.Url))
                .ToArray();
        }

        public IReadOnlyList<PlayerFeedRow> DefunctPlayerFeedRows { get; }
        public IReadOnlyList<PlayerFeed> NewPlayerFeeds { get; }

        public bool FoundChanges
            => DefunctPlayerFeedRows.Any() || NewPlayerFeeds.Any();
    }
}
