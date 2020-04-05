using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Scrapers.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BBallGraphs.Scrapers.Tests.BasketballReference
{
    [TestClass]
    public class IGameTests
    {
        [TestMethod]
        public void MatchesAndCopies()
        {
            var game1 = new Game
            {
                PlayerID = "jamesle01",
                PlayerName = "LeBron James",
                Season = 2004,
                Date = new DateTime(2003, 10, 29).AsUtc(),
                IsPlayoffGame = false,
                BoxScoreUrl = "https://www.basketball-reference.com/boxscores/200310290SAC.html",
                AgeInDays = 6877,
                Won = false,
                Started = true,
                SecondsPlayed = 2570,
                FieldGoalsMade = 12,
                FieldGoalsAttempted = 20,
                ThreePointersMade = 0,
                ThreePointersAttempted = 2,
                FreeThrowsMade = 1,
                FreeThrowsAttempted = 3,
                OffensiveRebounds = 2,
                DefensiveRebounds = 4,
                TotalRebounds = 6,
                Assists = 9,
                Steals = 4,
                Blocks = 0,
                Turnovers = 2,
                PersonalFouls = 3,
                Points = 25,
                GameScore = 24.7,
                PlusMinus = -9
            };

            var game2 = new Game
            {
                PlayerID = "jamesle01",
                PlayerName = "LeBron James",
                Season = 2004,
                Date = new DateTime(2003, 11, 05).AsUtc(),
                IsPlayoffGame = false,
                BoxScoreUrl = "https://www.basketball-reference.com/boxscores/200311050CLE.html",
                AgeInDays = 6884,
                Won = false,
                Started = true,
                SecondsPlayed = 2466,
                FieldGoalsMade = 3,
                FieldGoalsAttempted = 11,
                ThreePointersMade = 0,
                ThreePointersAttempted = 2,
                FreeThrowsMade = 1,
                FreeThrowsAttempted = 1,
                OffensiveRebounds = 2,
                DefensiveRebounds = 9,
                TotalRebounds = 11,
                Assists = 7,
                Steals = 2,
                Blocks = 3,
                Turnovers = 2,
                PersonalFouls = 1,
                Points = 7,
                GameScore = 11.2,
                PlusMinus = -3
            };

            Assert.IsTrue(game1.Matches(game1));
            Assert.IsFalse(game1.Matches(game2));
            Assert.IsFalse(game2.Matches(game1));

            game2.Date = new DateTime(2003, 10, 29).AsUtc();

            Assert.IsFalse(game1.Matches(game2));

            game1.CopyTo(game2);

            Assert.IsTrue(game1.Matches(game2));
            Assert.IsTrue(game2.Matches(game1));
            Assert.AreEqual(25, game1.Points);
            Assert.AreEqual(25, game2.Points);

            game2.GameScore = 11.2;

            Assert.IsFalse(game1.Matches(game2));
            Assert.IsFalse(game2.Matches(game1));
        }
    }
}
