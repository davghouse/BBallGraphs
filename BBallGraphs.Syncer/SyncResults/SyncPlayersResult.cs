using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Syncer.Rows;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.Syncer.SyncResults
{
    public class SyncPlayersResult
    {
        public SyncPlayersResult(
            IEnumerable<PlayerRow> playerRows,
            IEnumerable<Player> players)
        {
            var playerRowsDict = playerRows.ToDictionary(r => r.Url);
            var playersDict = players.ToDictionary(p => p.Url);

            DefunctPlayerRows = playerRowsDict.Values
                .Where(r => !playersDict.ContainsKey(r.Url))
                .ToArray();
            NewPlayers = playersDict.Values
                .Where(p => !playerRowsDict.ContainsKey(p.Url))
                .ToArray();

            var updatedPlayerRows = new List<PlayerRow>();
            foreach (var player in playersDict.Values)
            {
                if (playerRowsDict.TryGetValue(player.Url, out PlayerRow playerRow)
                    && !player.Matches(playerRow))
                {
                    player.CopyTo(playerRow);
                    updatedPlayerRows.Add(playerRow);
                }
            }
            UpdatedPlayerRows = updatedPlayerRows;
        }

        public IReadOnlyList<PlayerRow> DefunctPlayerRows { get; }
        public IReadOnlyList<Player> NewPlayers { get; }
        public IReadOnlyList<PlayerRow> UpdatedPlayerRows { get; }

        public bool FoundChanges
            => DefunctPlayerRows.Any() || NewPlayers.Any() || UpdatedPlayerRows.Any();
    }
}
