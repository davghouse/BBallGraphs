using BBallGraphs.AzureStorage.Rows;
using BBallGraphs.BasketballReferenceScraper;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.AzureStorage.SyncResults
{
    public class GamesSyncResult
    {
        public GamesSyncResult(
            IEnumerable<GameRow> gameRows,
            IEnumerable<Game> games)
        {
            var gameRowsByID = gameRows.ToDictionary(r => r.ID);
            var gamesByID = games.ToDictionary(g => g.ID);

            DefunctGameRows = gameRowsByID.Values
                .Where(r => !gamesByID.ContainsKey(r.ID))
                .ToArray();
            NewGameRows = GameRow.CreateRows(gamesByID.Values
                .Where(g => !gameRowsByID.ContainsKey(g.ID)))
                .ToArray();

            var updatedGameRows = new List<GameRow>();
            foreach (var game in gamesByID.Values)
            {
                if (gameRowsByID.TryGetValue(game.ID, out GameRow gameRow)
                    && !game.Matches(gameRow))
                {
                    game.CopyTo(gameRow);
                    updatedGameRows.Add(gameRow);
                }
            }
            UpdatedGameRows = updatedGameRows;
        }

        public IReadOnlyList<GameRow> DefunctGameRows { get; }
        public IReadOnlyList<GameRow> NewGameRows { get; }
        public IReadOnlyList<GameRow> UpdatedGameRows { get; }

        public bool FoundChanges
            => DefunctGameRows.Any() || NewGameRows.Any() || UpdatedGameRows.Any();
    }
}
