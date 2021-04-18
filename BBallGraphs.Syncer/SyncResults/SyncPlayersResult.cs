using BBallGraphs.BasketballReferenceScraper;
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
            var playerRowsDict = playerRows.ToDictionary(r => r.ID);
            var playersDict = players.ToDictionary(p => p.ID);

            DefunctPlayerRows = playerRowsDict.Values
                .Where(r => !playersDict.ContainsKey(r.ID))
                .ToArray();
            NewPlayers = playersDict.Values
                .Where(p => !playerRowsDict.ContainsKey(p.ID))
                .ToArray();

            var updatedPlayerRows = new List<PlayerRow>();
            foreach (var player in playersDict.Values)
            {
                if (playerRowsDict.TryGetValue(player.ID, out PlayerRow playerRow)
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
