using BBallGraphs.AzureStorage.Rows;
using BBallGraphs.BasketballReferenceScraper;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.AzureStorage.SyncResults
{
    public class PlayersSyncResult
    {
        public PlayersSyncResult(
            IEnumerable<PlayerRow> playerRows,
            IEnumerable<Player> players)
        {
            var playerRowsByID = playerRows.ToDictionary(r => r.ID);
            var playersByID = players.ToDictionary(p => p.ID);

            DefunctPlayerRows = playerRowsByID.Values
                .Where(r => !playersByID.ContainsKey(r.ID))
                .ToArray();
            NewPlayerRows = PlayerRow.CreateRows(playersByID.Values
                .Where(p => !playerRowsByID.ContainsKey(p.ID)))
                .ToArray();

            var updatedPlayerRows = new List<PlayerRow>();
            foreach (var player in playersByID.Values)
            {
                if (playerRowsByID.TryGetValue(player.ID, out PlayerRow playerRow)
                    && !player.Matches(playerRow))
                {
                    player.CopyTo(playerRow);
                    updatedPlayerRows.Add(playerRow);
                }
            }
            UpdatedPlayerRows = updatedPlayerRows;
        }

        public IReadOnlyList<PlayerRow> DefunctPlayerRows { get; }
        public IReadOnlyList<PlayerRow> NewPlayerRows { get; }
        public IReadOnlyList<PlayerRow> UpdatedPlayerRows { get; }

        public bool FoundChanges
            => DefunctPlayerRows.Any() || NewPlayerRows.Any() || UpdatedPlayerRows.Any();
    }
}
