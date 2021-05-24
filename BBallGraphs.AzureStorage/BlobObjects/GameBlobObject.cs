using BBallGraphs.BasketballReferenceScraper;
using System;

namespace BBallGraphs.AzureStorage.BlobObjects
{
    public class GameBlobObject : IEquatable<GameBlobObject>
    {
        public GameBlobObject()
        { }

        public GameBlobObject(IGame game)
        {
            ID = game.ID;
            PlayerID = game.PlayerID;
            PlayerName = game.PlayerName;
            Season = game.Season;
            Date = game.Date;
            Team = game.Team;
            OpponentTeam = game.OpponentTeam;
            IsHomeGame = game.IsHomeGame;
            IsPlayoffGame = game.IsPlayoffGame;
            BoxScoreUrl = game.BoxScoreUrl;
            AgeInDays = game.AgeInDays;
            Won = game.Won;
            Started = game.Started;
            SecondsPlayed = game.SecondsPlayed;
            FieldGoalsMade = game.FieldGoalsMade;
            FieldGoalsAttempted = game.FieldGoalsAttempted;
            ThreePointersMade = game.ThreePointersMade;
            ThreePointersAttempted = game.ThreePointersAttempted;
            FreeThrowsMade = game.FreeThrowsMade;
            FreeThrowsAttempted = game.FreeThrowsAttempted;
            OffensiveRebounds = game.OffensiveRebounds;
            DefensiveRebounds = game.DefensiveRebounds;
            TotalRebounds = game.TotalRebounds;
            Assists = game.Assists;
            Steals = game.Steals;
            Blocks = game.Blocks;
            Turnovers = game.Turnovers;
            PersonalFouls = game.PersonalFouls;
            Points = game.Points;
            GameScore = game.GameScore;
            PlusMinus = game.PlusMinus;
        }

        public string ID { get; set; }
        public string PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int Season { get; set; }
        public DateTime Date { get; set; }
        public string Team { get; set; }
        public string OpponentTeam { get; set; }
        public bool IsHomeGame { get; set; }
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

        public bool Equals(GameBlobObject other)
            => other != null
            && ID == other.ID
            && PlayerID == other.PlayerID
            && PlayerName == other.PlayerName
            && Season == other.Season
            && Date == other.Date
            && Team == other.Team
            && OpponentTeam == other.OpponentTeam
            && IsHomeGame == other.IsHomeGame
            && IsPlayoffGame == other.IsPlayoffGame
            && BoxScoreUrl == other.BoxScoreUrl
            && AgeInDays == other.AgeInDays
            && Won == other.Won
            && Started == other.Started
            && SecondsPlayed == other.SecondsPlayed
            && FieldGoalsMade == other.FieldGoalsMade
            && FieldGoalsAttempted == other.FieldGoalsAttempted
            && ThreePointersMade == other.ThreePointersMade
            && ThreePointersAttempted == other.ThreePointersAttempted
            && FreeThrowsMade == other.FreeThrowsMade
            && FreeThrowsAttempted == other.FreeThrowsAttempted
            && OffensiveRebounds == other.OffensiveRebounds
            && DefensiveRebounds == other.DefensiveRebounds
            && TotalRebounds == other.TotalRebounds
            && Assists == other.Assists
            && Steals == other.Steals
            && Blocks == other.Blocks
            && Turnovers == other.Turnovers
            && PersonalFouls == other.PersonalFouls
            && Points == other.Points
            && GameScore == other.GameScore
            && PlusMinus == other.PlusMinus;

        public override bool Equals(object obj)
            => Equals(obj as GameBlobObject);

        public override int GetHashCode()
            => throw new NotImplementedException();

        public override string ToString()
            => $"On {Date:d}, {PlayerName} had {Points}/{TotalRebounds}/{Assists}";
    }
}
