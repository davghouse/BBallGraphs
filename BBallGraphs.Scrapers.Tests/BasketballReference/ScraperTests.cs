using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Scrapers.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BBallGraphs.Scrapers.Tests.BasketballReference
{
    [TestClass]
    public class ScraperTests
    {
        [TestMethod]
        public async Task GetPlayerFeeds()
        {
            var playerFeeds = await Scraper.GetPlayerFeeds();

            Assert.IsTrue(playerFeeds.Count >= 25 && playerFeeds.Count <= 26
                && playerFeeds.First().Url.EndsWith("/a/")
                && playerFeeds.Last().Url.EndsWith("/z/"));
        }

        [TestMethod]
        public async Task GetPlayers()
        {
            var players = await Scraper.GetPlayers(
                new PlayerFeed { Url = "https://www.basketball-reference.com/players/a/" });

            Assert.IsTrue(players.Count >= 166);
            Assert.AreEqual(players.Count, players.Select(p => p.ID).Distinct().Count());
            Assert.AreEqual(1, players.Count(p => p.ID == "abdelal01"
                && p.Name == "Alaa Abdelnaby" && p.FirstSeason == 1991 && p.LastSeason == 1995
                && p.HeightInInches == 82 && p.BirthDate == new DateTime(1968, 6, 24).AsUtc()));
            Assert.AreEqual(1, players.Count(p => p.ID == "azubuke01"
                && p.Name == "Kelenna Azubuike" && p.FirstSeason == 2007 && p.LastSeason == 2012
                && p.HeightInInches == 77 && p.BirthDate == new DateTime(1983, 12, 16).AsUtc()));
            // Use Utc as an arbitrary but consistent time zone (and so Azure doesn't try to convert to it).
            Assert.IsTrue(players.All(p => p.BirthDate.Kind == DateTimeKind.Utc));
            Assert.IsTrue(players.All(p => p.BirthDate.TimeOfDay == TimeSpan.Zero));
        }

        [TestMethod]
        public async Task GetGamesForModernPlayer_WhoMissedThePlayoffs()
        {
            var player = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                ID = "jamesle01",
                Name = "LeBron James",
                FirstSeason = 2004,
                LastSeason = 2020,
                BirthDate = new DateTime(1984, 12, 30).AsUtc()
            };
            var games = await Scraper.GetGames(player, 2004);

            Assert.AreEqual(79, games.Count);
            Assert.AreEqual(38, games.Where(g => g.IsHomeGame).Count());
            Assert.IsFalse(games.Any(g => g.IsPlayoffGame));
            Assert.IsTrue(games.All(g => g.Points > 0));

            Assert.IsTrue(games[0].Matches(new Game
            {
                PlayerID = "jamesle01",
                PlayerName = "LeBron James",
                Season = 2004,
                Date = new DateTime(2003, 10, 29).AsUtc(),
                Team = "CLE",
                OpponentTeam = "SAC",
                IsHomeGame = false,
                IsPlayoffGame = false,
                BoxScoreUrl = "https://www.basketball-reference.com/boxscores/200310290SAC.html",
                ID = "jamesle01 10/29/2003",
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
            }));

            Assert.IsTrue(games[3].Matches(new Game
            {
                PlayerID = "jamesle01",
                PlayerName = "LeBron James",
                Season = 2004,
                Date = new DateTime(2003, 11, 05).AsUtc(),
                Team = "CLE",
                OpponentTeam = "DEN",
                IsHomeGame = true,
                IsPlayoffGame = false,
                BoxScoreUrl = "https://www.basketball-reference.com/boxscores/200311050CLE.html",
                ID = "jamesle01 11/5/2003",
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
            }));

            Assert.AreEqual(622, games.Sum(g => g.FieldGoalsMade));
            Assert.AreEqual(1492, games.Sum(g => g.FieldGoalsAttempted));
            Assert.AreEqual(63, games.Sum(g => g.ThreePointersMade));
            Assert.AreEqual(217, games.Sum(g => g.ThreePointersAttempted));
            Assert.AreEqual(347, games.Sum(g => g.FreeThrowsMade));
            Assert.AreEqual(460, games.Sum(g => g.FreeThrowsAttempted));
            Assert.AreEqual(99, games.Sum(g => g.OffensiveRebounds));
            Assert.AreEqual(333, games.Sum(g => g.DefensiveRebounds));
            Assert.AreEqual(432, games.Sum(g => g.TotalRebounds));
            Assert.AreEqual(465, games.Sum(g => g.Assists));
            Assert.AreEqual(130, games.Sum(g => g.Steals));
            Assert.AreEqual(58, games.Sum(g => g.Blocks));
            Assert.AreEqual(273, games.Sum(g => g.Turnovers));
            Assert.AreEqual(149, games.Sum(g => g.PersonalFouls));
            Assert.AreEqual(1654, games.Sum(g => g.Points));
            Assert.AreEqual(1145, (int)games.Sum(g => g.GameScore));
            Assert.AreEqual(-144, games.Sum(g => g.PlusMinus));
        }

        [TestMethod]
        public async Task GetGamesForModernPlayer_WhoMadeThePlayoffs()
        {
            var player = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                ID = "jamesle01",
                Name = "LeBron James",
                FirstSeason = 2004,
                LastSeason = 2020,
                BirthDate = new DateTime(1984, 12, 30).AsUtc()
            };
            var games = await Scraper.GetGames(player, 2016);
            var regularSeasonGames = games.Where(g => !g.IsPlayoffGame);
            var playoffGames = games.Where(g => g.IsPlayoffGame);

            Assert.AreEqual(76, regularSeasonGames.Count());
            Assert.AreEqual(38, regularSeasonGames.Where(g => g.IsHomeGame).Count());
            Assert.AreEqual(737, regularSeasonGames.Sum(g => g.FieldGoalsMade));
            Assert.AreEqual(1416, regularSeasonGames.Sum(g => g.FieldGoalsAttempted));
            Assert.AreEqual(87, regularSeasonGames.Sum(g => g.ThreePointersMade));
            Assert.AreEqual(282, regularSeasonGames.Sum(g => g.ThreePointersAttempted));
            Assert.AreEqual(359, regularSeasonGames.Sum(g => g.FreeThrowsMade));
            Assert.AreEqual(491, regularSeasonGames.Sum(g => g.FreeThrowsAttempted));
            Assert.AreEqual(111, regularSeasonGames.Sum(g => g.OffensiveRebounds));
            Assert.AreEqual(454, regularSeasonGames.Sum(g => g.DefensiveRebounds));
            Assert.AreEqual(565, regularSeasonGames.Sum(g => g.TotalRebounds));
            Assert.AreEqual(514, regularSeasonGames.Sum(g => g.Assists));
            Assert.AreEqual(104, regularSeasonGames.Sum(g => g.Steals));
            Assert.AreEqual(49, regularSeasonGames.Sum(g => g.Blocks));
            Assert.AreEqual(249, regularSeasonGames.Sum(g => g.Turnovers));
            Assert.AreEqual(143, regularSeasonGames.Sum(g => g.PersonalFouls));
            Assert.AreEqual(1920, regularSeasonGames.Sum(g => g.Points));

            Assert.AreEqual(21, playoffGames.Count());
            Assert.AreEqual(10, playoffGames.Where(g => g.IsHomeGame).Count());
            Assert.AreEqual(219, playoffGames.Sum(g => g.FieldGoalsMade));
            Assert.AreEqual(417, playoffGames.Sum(g => g.FieldGoalsAttempted));
            Assert.AreEqual(32, playoffGames.Sum(g => g.ThreePointersMade));
            Assert.AreEqual(94, playoffGames.Sum(g => g.ThreePointersAttempted));
            Assert.AreEqual(82, playoffGames.Sum(g => g.FreeThrowsMade));
            Assert.AreEqual(124, playoffGames.Sum(g => g.FreeThrowsAttempted));
            Assert.AreEqual(42, playoffGames.Sum(g => g.OffensiveRebounds));
            Assert.AreEqual(158, playoffGames.Sum(g => g.DefensiveRebounds));
            Assert.AreEqual(200, playoffGames.Sum(g => g.TotalRebounds));
            Assert.AreEqual(160, playoffGames.Sum(g => g.Assists));
            Assert.AreEqual(49, playoffGames.Sum(g => g.Steals));
            Assert.AreEqual(27, playoffGames.Sum(g => g.Blocks));
            Assert.AreEqual(75, playoffGames.Sum(g => g.Turnovers));
            Assert.AreEqual(54, playoffGames.Sum(g => g.PersonalFouls));
            Assert.AreEqual(552, playoffGames.Sum(g => g.Points));
        }

        [TestMethod]
        public async Task GetGamesForOldPlayerWithIncompleteStats()
        {
            var player = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/b/",
                ID = "braunca01",
                Name = "Carl Braun",
                FirstSeason = 1948,
                LastSeason = 1962,
                BirthDate = new DateTime(1927, 9, 25).AsUtc()
            };
            var games = await Scraper.GetGames(player, 1949);
            var regularSeasonGames = games.Where(g => !g.IsPlayoffGame);
            var playoffGames = games.Where(g => g.IsPlayoffGame);

            Assert.IsTrue(games[0].Matches(new Game
            {
                PlayerID = "braunca01",
                PlayerName = "Carl Braun",
                Season = 1949,
                Date = new DateTime(1948, 11, 03).AsUtc(),
                Team = "NYK",
                OpponentTeam = "FTW",
                IsHomeGame = false,
                IsPlayoffGame = false,
                BoxScoreUrl = "https://www.basketball-reference.com/boxscores/194811030FTW.html",
                ID = "braunca01 11/3/1948",
                AgeInDays = 7710,
                Won = true,
                Started = null,
                SecondsPlayed = null,
                FieldGoalsMade = 2,
                FieldGoalsAttempted = null,
                ThreePointersMade = null,
                ThreePointersAttempted = null,
                FreeThrowsMade = 6,
                FreeThrowsAttempted = 7,
                OffensiveRebounds = null,
                DefensiveRebounds = null,
                TotalRebounds = null,
                Assists = null,
                Steals = null,
                Blocks = null,
                Turnovers = null,
                PersonalFouls = null,
                Points = 10,
                GameScore = null,
                PlusMinus = null
            }));

            // The totals on a player's main Basketball Reference page can disagree with the totals gotten by
            // summing their game logs. For example, summing the game logs here gives 813 points, but it's 810
            // in the player's Totals grid. Also, the Totals grid might have more data. Like for this player
            // season it has data for field goal attempts, assists, and personal fouls, but the game logs don't.
            // I'm gonna assume these discrepancies don't exist for modern players, so I won't worry about them.
            Assert.AreEqual(57, regularSeasonGames.Count());
            Assert.AreEqual(29, regularSeasonGames.Where(g => g.IsHomeGame).Count());
            Assert.AreEqual(300, regularSeasonGames.NullableSum(g => g.FieldGoalsMade));
            Assert.AreEqual(54, regularSeasonGames.NullableSum(g => g.FieldGoalsAttempted));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.ThreePointersMade));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.ThreePointersAttempted));
            Assert.AreEqual(213, regularSeasonGames.NullableSum(g => g.FreeThrowsMade));
            Assert.AreEqual(283, regularSeasonGames.NullableSum(g => g.FreeThrowsAttempted));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.OffensiveRebounds));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.DefensiveRebounds));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.TotalRebounds));
            Assert.AreEqual(17, regularSeasonGames.NullableSum(g => g.Assists));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.Steals));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.Blocks));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.Turnovers));
            Assert.AreEqual(8, regularSeasonGames.NullableSum(g => g.PersonalFouls));
            Assert.AreEqual(813, regularSeasonGames.NullableSum(g => g.Points));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.GameScore));
            Assert.AreEqual(null, regularSeasonGames.NullableSum(g => g.PlusMinus));

            Assert.AreEqual(6, playoffGames.Count());
            Assert.AreEqual(3, playoffGames.Where(g => g.IsHomeGame).Count());
            Assert.AreEqual(33, playoffGames.NullableSum(g => g.FieldGoalsMade));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.FieldGoalsAttempted));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.ThreePointersMade));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.ThreePointersAttempted));
            Assert.AreEqual(50, playoffGames.NullableSum(g => g.FreeThrowsMade));
            Assert.AreEqual(62, playoffGames.NullableSum(g => g.FreeThrowsAttempted));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.OffensiveRebounds));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.DefensiveRebounds));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.TotalRebounds));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.Assists));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.Steals));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.Blocks));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.Turnovers));
            Assert.AreEqual(22, playoffGames.NullableSum(g => g.PersonalFouls));
            Assert.AreEqual(116, playoffGames.NullableSum(g => g.Points));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.GameScore));
            Assert.AreEqual(null, playoffGames.NullableSum(g => g.PlusMinus));
        }

        [TestMethod]
        public async Task GetGamesForPlayerWhoMissedEntireSeason_DueToRetirement()
        {
            var player = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                ID = "jordami01",
                Name = "Michael Jordan",
                FirstSeason = 1985,
                LastSeason = 2003,
                BirthDate = new DateTime(1963, 2, 17).AsUtc()
            };
            var games = await Scraper.GetGames(player, 1994);
            var regularSeasonGames = games.Where(g => !g.IsPlayoffGame);
            var playoffGames = games.Where(g => g.IsPlayoffGame);

            Assert.AreEqual(0, regularSeasonGames.Count());
            Assert.AreEqual(0, playoffGames.Count());
        }

        [TestMethod]
        public async Task GetGamesForPlayerWhoMissedEntireSeason_DueToInjury()
        {
            var player = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/o/",
                ID = "odengr01",
                Name = "Greg Oden",
                FirstSeason = 2008,
                LastSeason = 2014,
                BirthDate = new DateTime(1963, 2, 17).AsUtc()
            };
            var games = await Scraper.GetGames(player, 2008);
            var regularSeasonGames = games.Where(g => !g.IsPlayoffGame);
            var playoffGames = games.Where(g => g.IsPlayoffGame);

            Assert.AreEqual(0, regularSeasonGames.Count());
            Assert.AreEqual(0, playoffGames.Count());
        }

        [TestMethod]
        public async Task GetGamesForAPlayerWhoPlayedForBothTeamsInTheSameGame()
        {
            var player = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/m/",
                ID = "moneyer01",
                Name = "Eric Money",
                FirstSeason = 1975,
                LastSeason = 1980,
                BirthDate = new DateTime(1955, 2, 6).AsUtc()
            };
            var games = await Scraper.GetGames(player, 1979);
            var regularSeasonGames = games.Where(g => !g.IsPlayoffGame);
            var playoffGames = games.Where(g => g.IsPlayoffGame);

            Assert.AreEqual(69, regularSeasonGames.Count());
            Assert.AreEqual(33, regularSeasonGames.Where(g => g.IsHomeGame).Count());
            Assert.AreEqual(8, playoffGames.Count());
            Assert.AreEqual(4, playoffGames.Where(g => g.IsHomeGame).Count());

            var mergedGameData = games.Single(g => g.Date == new DateTime(1978, 11, 8).AsUtc());

            Assert.AreEqual("PHI", mergedGameData.Team);
            Assert.AreEqual("NJN", mergedGameData.OpponentTeam);
            Assert.IsTrue(mergedGameData.IsHomeGame);
            Assert.IsTrue(mergedGameData.Won);
            Assert.IsTrue(mergedGameData.Started.Value);
            Assert.AreEqual(27, mergedGameData.Points);
            Assert.AreEqual(3, mergedGameData.TotalRebounds);
            Assert.AreEqual(6, mergedGameData.Assists);
            Assert.AreEqual(1, mergedGameData.FreeThrowsMade);
            Assert.AreEqual(2, mergedGameData.FreeThrowsAttempted);
        }
    }
}
