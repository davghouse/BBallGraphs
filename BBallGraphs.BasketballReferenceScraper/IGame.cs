using System;
using System.Collections.Generic;

namespace BBallGraphs.BasketballReferenceScraper
{
    public interface IGame
    {
        string ID { get; set; }
        string PlayerID { get; set; }
        string PlayerName { get; set; }
        int Season { get; set; }
        DateTime Date { get; set; }
        string Team { get; set; }
        string OpponentTeam { get; set; }
        bool IsHomeGame { get; set; }
        bool IsPlayoffGame { get; set; }
        string BoxScoreUrl { get; set; }
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
            => game.ID == otherGame.ID
            && game.PlayerID == otherGame.PlayerID
            && game.PlayerName == otherGame.PlayerName
            && game.Season == otherGame.Season
            && game.Date == otherGame.Date
            && game.Team == otherGame.Team
            && game.OpponentTeam == otherGame.OpponentTeam
            && game.IsHomeGame == otherGame.IsHomeGame
            && game.IsPlayoffGame == otherGame.IsPlayoffGame
            && game.BoxScoreUrl == otherGame.BoxScoreUrl
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
            targetGame.ID = sourceGame.ID;
            targetGame.PlayerID = sourceGame.PlayerID;
            targetGame.PlayerName = sourceGame.PlayerName;
            targetGame.Season = sourceGame.Season;
            targetGame.Date = sourceGame.Date;
            targetGame.Team = sourceGame.Team;
            targetGame.OpponentTeam = sourceGame.OpponentTeam;
            targetGame.IsHomeGame = sourceGame.IsHomeGame;
            targetGame.IsPlayoffGame = sourceGame.IsPlayoffGame;
            targetGame.BoxScoreUrl = sourceGame.BoxScoreUrl;
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

        public static IEnumerable<string> GetUpdatedFields(this IGame sourceGame, IGame targetGame)
        {
            if (sourceGame.ID != targetGame.ID) yield return nameof(IGame.ID);
            if (sourceGame.PlayerID != targetGame.PlayerID) yield return nameof(IGame.PlayerID);
            if (sourceGame.PlayerName != targetGame.PlayerName) yield return nameof(IGame.PlayerName);
            if (sourceGame.Season != targetGame.Season) yield return nameof(IGame.Season);
            if (sourceGame.Date != targetGame.Date) yield return nameof(IGame.Date);
            if (sourceGame.Team != targetGame.Team) yield return nameof(IGame.Team);
            if (sourceGame.OpponentTeam != targetGame.OpponentTeam) yield return nameof(IGame.OpponentTeam);
            if (sourceGame.IsHomeGame != targetGame.IsHomeGame) yield return nameof(IGame.IsHomeGame);
            if (sourceGame.IsPlayoffGame != targetGame.IsPlayoffGame) yield return nameof(IGame.IsPlayoffGame);
            if (sourceGame.BoxScoreUrl != targetGame.BoxScoreUrl) yield return nameof(IGame.BoxScoreUrl);
            if (sourceGame.AgeInDays != targetGame.AgeInDays) yield return nameof(IGame.AgeInDays);
            if (sourceGame.Won != targetGame.Won) yield return nameof(IGame.Won);
            if (sourceGame.Started != targetGame.Started) yield return nameof(IGame.Started);
            if (sourceGame.SecondsPlayed != targetGame.SecondsPlayed) yield return nameof(IGame.SecondsPlayed);
            if (sourceGame.FieldGoalsMade != targetGame.FieldGoalsMade) yield return nameof(IGame.FieldGoalsMade);
            if (sourceGame.FieldGoalsAttempted != targetGame.FieldGoalsAttempted) yield return nameof(IGame.FieldGoalsAttempted);
            if (sourceGame.ThreePointersMade != targetGame.ThreePointersMade) yield return nameof(IGame.ThreePointersMade);
            if (sourceGame.ThreePointersAttempted != targetGame.ThreePointersAttempted) yield return nameof(IGame.ThreePointersAttempted);
            if (sourceGame.FreeThrowsMade != targetGame.FreeThrowsMade) yield return nameof(IGame.FreeThrowsMade);
            if (sourceGame.FreeThrowsAttempted != targetGame.FreeThrowsAttempted) yield return nameof(IGame.FreeThrowsAttempted);
            if (sourceGame.OffensiveRebounds != targetGame.OffensiveRebounds) yield return nameof(IGame.OffensiveRebounds);
            if (sourceGame.DefensiveRebounds != targetGame.DefensiveRebounds) yield return nameof(IGame.DefensiveRebounds);
            if (sourceGame.TotalRebounds != targetGame.TotalRebounds) yield return nameof(IGame.TotalRebounds);
            if (sourceGame.Assists != targetGame.Assists) yield return nameof(IGame.Assists);
            if (sourceGame.Steals != targetGame.Steals) yield return nameof(IGame.Steals);
            if (sourceGame.Blocks != targetGame.Blocks) yield return nameof(IGame.Blocks);
            if (sourceGame.Turnovers != targetGame.Turnovers) yield return nameof(IGame.Turnovers);
            if (sourceGame.PersonalFouls != targetGame.PersonalFouls) yield return nameof(IGame.PersonalFouls);
            if (sourceGame.Points != targetGame.Points) yield return nameof(IGame.Points);
            if (sourceGame.GameScore != targetGame.GameScore) yield return nameof(IGame.GameScore);
            if (sourceGame.PlusMinus != targetGame.PlusMinus) yield return nameof(IGame.PlusMinus);
        }
    }
}
