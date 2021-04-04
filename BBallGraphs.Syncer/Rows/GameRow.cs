using BBallGraphs.Scrapers.BasketballReference;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BBallGraphs.Syncer.Rows
{
    public class GameRow : TableEntity, IGame
    {
        public string PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int Season { get; set; }
        public DateTime Date { get; set; }
        public string Team { get; set; }
        public string OpponentTeam { get; set; }
        public bool IsHomeGame { get; set; }
        public bool IsPlayoffGame { get; set; }
        public string BoxScoreUrl { get; set; }
        public string ID { get; set; }
        public int AgeInDays { get; set; }
        public bool Won { get; set; }
        public bool? Started { get; set; }
        public int? SecondsPlayed { get; set; }
        public int? FieldGoalsMade { get; set; }
        public int? FieldGoalsAttempted { get; set; }
        public int? ThreePointersMade { get; set; }
        public int? ThreePointersAttempted { get; set; }
        public int? FreeThrowsMade { get; set; }
        public int? FreeThrowsAttempted { get; set; }
        public int? OffensiveRebounds { get; set; }
        public int? DefensiveRebounds { get; set; }
        public int? TotalRebounds { get; set; }
        public int? Assists { get; set; }
        public int? Steals { get; set; }
        public int? Blocks { get; set; }
        public int? Turnovers { get; set; }
        public int? PersonalFouls { get; set; }
        public int Points { get; set; }
        public double? GameScore { get; set; }
        public int? PlusMinus { get; set; }

        public static GameRow CreateRow(IGame game)
        {
            var gameRow = new GameRow
            {
                PartitionKey = game.PlayerID,
                RowKey = GetRowKey(game.Date)
            };
            game.CopyTo(gameRow);

            return gameRow;
        }

        public static IEnumerable<GameRow> CreateRows(IEnumerable<IGame> games)
            => games.Select(CreateRow);

        public static string GetRowKey(DateTime date)
            => date.Ticks.ToString("D19");

        public override string ToString()
            => $"On {Date:d}, {PlayerName} had {Points}/{TotalRebounds}/{Assists}";
    }
}
