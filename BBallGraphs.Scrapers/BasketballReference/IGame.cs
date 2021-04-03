using System;

namespace BBallGraphs.Scrapers.BasketballReference
{
    public interface IGame
    {
        string PlayerID { get; set; }
        string PlayerName { get; set; }
        int Season { get; set; }
        DateTime Date { get; set; }
        bool IsPlayoffGame { get; set; }
        string BoxScoreUrl { get; set; }
        string ID { get; set; }
        int AgeInDays { get; set; }
        bool Won { get; set; }
        bool? Started { get; set; }
        int? SecondsPlayed { get; set; }
        int? FieldGoalsMade { get; set; }
        int? FieldGoalsAttempted { get; set; }
        int? ThreePointersMade { get; set; }
        int? ThreePointersAttempted { get; set; }
        int? FreeThrowsMade { get; set; }
        int? FreeThrowsAttempted { get; set; }
        int? OffensiveRebounds { get; set; }
        int? DefensiveRebounds { get; set; }
        int? TotalRebounds { get; set; }
        int? Assists { get; set; }
        int? Steals { get; set; }
        int? Blocks { get; set; }
        int? Turnovers { get; set; }
        int? PersonalFouls { get; set; }
        int Points { get; set; }
        double? GameScore { get; set; }
        int? PlusMinus { get; set; }
    }

    public static class IGameExtensions
    {
        public static bool Matches(this IGame game, IGame otherGame)
            => game.PlayerID == otherGame.PlayerID
            && game.PlayerName == otherGame.PlayerName
            && game.Season == otherGame.Season
            && game.Date == otherGame.Date
            && game.IsPlayoffGame == otherGame.IsPlayoffGame
            && game.BoxScoreUrl == otherGame.BoxScoreUrl
            && game.ID == otherGame.ID
            && game.AgeInDays == otherGame.AgeInDays
            && game.Won == otherGame.Won
            && game.Started == otherGame.Started
            && game.SecondsPlayed == otherGame.SecondsPlayed
            && game.FieldGoalsMade == otherGame.FieldGoalsMade
            && game.FieldGoalsAttempted == otherGame.FieldGoalsAttempted
            && game.ThreePointersMade == otherGame.ThreePointersMade
            && game.ThreePointersAttempted == otherGame.ThreePointersAttempted
            && game.FreeThrowsMade == otherGame.FreeThrowsMade
            && game.FreeThrowsAttempted == otherGame.FreeThrowsAttempted
            && game.OffensiveRebounds == otherGame.OffensiveRebounds
            && game.DefensiveRebounds == otherGame.DefensiveRebounds
            && game.TotalRebounds == otherGame.TotalRebounds
            && game.Assists == otherGame.Assists
            && game.Steals == otherGame.Steals
            && game.Blocks == otherGame.Blocks
            && game.Turnovers == otherGame.Turnovers
            && game.PersonalFouls == otherGame.PersonalFouls
            && game.Points == otherGame.Points
            && game.GameScore == otherGame.GameScore
            && game.PlusMinus == otherGame.PlusMinus;

        public static void CopyTo(this IGame sourceGame, IGame targetGame)
        {
            targetGame.PlayerID = sourceGame.PlayerID;
            targetGame.PlayerName = sourceGame.PlayerName;
            targetGame.Season = sourceGame.Season;
            targetGame.Date = sourceGame.Date;
            targetGame.IsPlayoffGame = sourceGame.IsPlayoffGame;
            targetGame.BoxScoreUrl = sourceGame.BoxScoreUrl;
            targetGame.ID = sourceGame.ID;
            targetGame.AgeInDays = sourceGame.AgeInDays;
            targetGame.Won = sourceGame.Won;
            targetGame.Started = sourceGame.Started;
            targetGame.SecondsPlayed = sourceGame.SecondsPlayed;
            targetGame.FieldGoalsMade = sourceGame.FieldGoalsMade;
            targetGame.FieldGoalsAttempted = sourceGame.FieldGoalsAttempted;
            targetGame.ThreePointersMade = sourceGame.ThreePointersMade;
            targetGame.ThreePointersAttempted = sourceGame.ThreePointersAttempted;
            targetGame.FreeThrowsMade = sourceGame.FreeThrowsMade;
            targetGame.FreeThrowsAttempted = sourceGame.FreeThrowsAttempted;
            targetGame.OffensiveRebounds = sourceGame.OffensiveRebounds;
            targetGame.DefensiveRebounds = sourceGame.DefensiveRebounds;
            targetGame.TotalRebounds = sourceGame.TotalRebounds;
            targetGame.Assists = sourceGame.Assists;
            targetGame.Steals = sourceGame.Steals;
            targetGame.Blocks = sourceGame.Blocks;
            targetGame.Turnovers = sourceGame.Turnovers;
            targetGame.PersonalFouls = sourceGame.PersonalFouls;
            targetGame.Points = sourceGame.Points;
            targetGame.GameScore = sourceGame.GameScore;
            targetGame.PlusMinus = sourceGame.PlusMinus;
        }
    }
}
