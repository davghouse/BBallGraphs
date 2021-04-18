using BBallGraphs.BasketballReferenceScraper;
using BBallGraphs.Syncer.Rows;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.Syncer.SyncResults
{
    public class SyncGamesResult
    {
        public SyncGamesResult(
            IEnumerable<GameRow> gameRows,
            IEnumerable<Game> games)
        {
            var gameRowsDict = gameRows.ToDictionary(r => r.ID);
            var gamesDict = games.ToDictionary(g => g.ID);

            DefunctGameRows = gameRowsDict.Values
                .Where(r => !gamesDict.ContainsKey(r.ID))
                .ToArray();
            NewGames = gamesDict.Values
                .Where(g => !gameRowsDict.ContainsKey(g.ID))
                .ToArray();

            var updatedGameRows = new List<GameRow>();
            foreach (var game in gamesDict.Values)
            {
                if (gameRowsDict.TryGetValue(game.ID, out GameRow gameRow)
                    && !game.Matches(gameRow))
                {
                    game.CopyTo(gameRow);
                    updatedGameRows.Add(gameRow);
                }
            }
            UpdatedGameRows = updatedGameRows;
        }

        public IReadOnlyList<GameRow> DefunctGameRows { get; }
        public IReadOnlyList<Game> NewGames { get; }
        public IReadOnlyList<GameRow> UpdatedGameRows { get; }

        public bool FoundChanges
            => DefunctGameRows.Any() || NewGames.Any() || UpdatedGameRows.Any();
    }
}
