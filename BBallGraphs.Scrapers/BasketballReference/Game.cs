using System;

namespace BBallGraphs.Scrapers.BasketballReference
{
    public class Game : IGame
    {
        public string PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int Season { get; set; }
        public DateTime Date { get; set; }
        public bool IsPlayoffGame { get; set; }
        public string BoxScoreUrl { get; set; }
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

        public override string ToString()
            => $"On {Date:d}, {PlayerName} had {Points}/{TotalRebounds}/{Assists}";
    }
}
