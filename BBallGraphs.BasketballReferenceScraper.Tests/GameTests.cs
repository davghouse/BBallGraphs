using BBallGraphs.BasketballReferenceScraper.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BBallGraphs.BasketballReferenceScraper.Tests
{
    [TestClass]
    public class GameTests
    {
        [TestMethod]
        public void ToStringFormatting()
        {
            var game = new Game
            {
                Date = new DateTime(2012, 6, 7).AsUtc(),
                PlayerName = "LeBron James",
                Points = 45,
                TotalRebounds = 15,
                Assists = 5
            };

            Assert.AreEqual("On 6/7/2012, LeBron James had 45/15/5", game.ToString());
        }

        [TestMethod]
        public void MergeSplitGameData()
        {
            var firstGameData = new Game
            {
                ID = "testte01 1/1/1990",
                PlayerID = "testte01",
                PlayerName = "test test",
                Season = 1990,
                Date = new DateTime(1990, 1, 1).AsUtc(),
                Team = "CLE",
                OpponentTeam = "GSW",
                IsHomeGame = false,
                IsPlayoffGame = false,
                BoxScoreUrl = "https://www.basketball-reference.com/boxscores/19900101CLE.html",
                AgeInDays = 10000,
                Won = false,
                Started = true,
                SecondsPlayed = 600,
                FieldGoalsMade = 10,
                FieldGoalsAttempted = 20,
                ThreePointersMade = 1,
                ThreePointersAttempted = 2,
                FreeThrowsMade = 2,
                FreeThrowsAttempted = 3,
                OffensiveRebounds = 4,
                DefensiveRebounds = 6,
                TotalRebounds = 10,
                Assists = null,
                Steals = 1,
                Blocks = 2,
                Turnovers = 3,
                PersonalFouls = 4,
                Points = 20,
                GameScore = 15.6,
                PlusMinus = null
            };

            var secondGameData = new Game
            {
                ID = "testte01 1/1/1990",
                PlayerID = "testte01",
                PlayerName = "test test",
                Season = 1990,
                Date = new DateTime(1990, 1, 1).AsUtc(),
                Team = "GSW",
                OpponentTeam = "CLE",
                IsHomeGame = true,
                IsPlayoffGame = false,
                BoxScoreUrl = "https://www.basketball-reference.com/boxscores/19900101CLE.html",
                AgeInDays = 10000,
                Won = true,
                Started = false,
                SecondsPlayed = 400,
                FieldGoalsMade = 5,
                FieldGoalsAttempted = 10,
                ThreePointersMade = 2,
                ThreePointersAttempted = 4,
                FreeThrowsMade = 0,
                FreeThrowsAttempted = 2,
                OffensiveRebounds = null,
                DefensiveRebounds = null,
                TotalRebounds = null,
                Assists = 15,
                Steals = 2,
                Blocks = 3,
                Turnovers = 4,
                PersonalFouls = 5,
                Points = 10,
                GameScore = 19.9,
                PlusMinus = null
            };

            var mergedGameData = Game.MergeSplitGameData(firstGameData, secondGameData);

            Assert.AreEqual("testte01 1/1/1990", mergedGameData.ID);
            Assert.AreEqual("testte01", mergedGameData.PlayerID);
            Assert.AreEqual("test test", mergedGameData.PlayerName);
            Assert.AreEqual(1990, mergedGameData.Season);
            Assert.AreEqual(new DateTime(1990, 1, 1).AsUtc(), mergedGameData.Date);
            Assert.AreEqual("GSW", mergedGameData.Team);
            Assert.AreEqual("CLE", mergedGameData.OpponentTeam);
            Assert.IsTrue(mergedGameData.IsHomeGame);
            Assert.IsFalse(mergedGameData.IsPlayoffGame);
            Assert.AreEqual("https://www.basketball-reference.com/boxscores/19900101CLE.html", mergedGameData.BoxScoreUrl);
            Assert.AreEqual(10000, mergedGameData.AgeInDays);
            Assert.IsTrue(mergedGameData.Won);
            Assert.IsTrue(mergedGameData.Started.Value);
            Assert.AreEqual(1000, mergedGameData.SecondsPlayed);
            Assert.AreEqual(15, mergedGameData.FieldGoalsMade);
            Assert.AreEqual(30, mergedGameData.FieldGoalsAttempted);
            Assert.AreEqual(3, mergedGameData.ThreePointersMade);
            Assert.AreEqual(6, mergedGameData.ThreePointersAttempted);
            Assert.AreEqual(2, mergedGameData.FreeThrowsMade);
            Assert.AreEqual(5, mergedGameData.FreeThrowsAttempted);
            Assert.AreEqual(4, mergedGameData.OffensiveRebounds);
            Assert.AreEqual(6, mergedGameData.DefensiveRebounds);
            Assert.AreEqual(10, mergedGameData.TotalRebounds);
            Assert.AreEqual(15, mergedGameData.Assists);
            Assert.AreEqual(3, mergedGameData.Steals);
            Assert.AreEqual(5, mergedGameData.Blocks);
            Assert.AreEqual(7, mergedGameData.Turnovers);
            Assert.AreEqual(9, mergedGameData.PersonalFouls);
            Assert.AreEqual(30, mergedGameData.Points);
            Assert.AreEqual(35.5, mergedGameData.GameScore);
            Assert.AreEqual(null, mergedGameData.PlusMinus);
        }
    }
}
