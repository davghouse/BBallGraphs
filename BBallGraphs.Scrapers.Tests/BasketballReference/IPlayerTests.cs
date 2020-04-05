using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Scrapers.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BBallGraphs.Scrapers.Tests.BasketballReference
{
    [TestClass]
    public class IPlayerTests
    {
        [TestMethod]
        public void Matches()
        {
            var player1 = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                ID = "jamesle01",
                Name = "LeBron James",
                FirstSeason = 2004,
                LastSeason = 2020,
                Position = "F-G",
                HeightInInches = 81,
                WeightInPounds = 250,
                BirthDate = new DateTime(1984, 12, 30).AsUtc(),
            };
            var player2 = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/l/",
                ID = "lamesje01",
                Name = "JeBron Lames",
                FirstSeason = 2020,
                LastSeason = 2036,
                Position = "C",
                HeightInInches = 90,
                WeightInPounds = 350,
                BirthDate = new DateTime(2004, 12, 30).AsUtc(),
            };

            Assert.IsTrue(player1.Matches(player1));
            Assert.IsFalse(player1.Matches(player2));
            Assert.IsFalse(player2.Matches(player1));

            player2.FeedUrl = "https://www.basketball-reference.com/players/j/";
            player2.ID = "jamesle01";
            player2.Name = "LeBron James";
            player2.FirstSeason = 2004;
            player2.Position = "F-G";
            player2.HeightInInches = 81;
            player2.BirthDate = new DateTime(1984, 12, 30).AsUtc();

            Assert.IsFalse(player1.Matches(player2));
            Assert.IsFalse(player2.Matches(player1));

            player2.LastSeason = 2020;
            player2.WeightInPounds = 250;

            Assert.IsTrue(player1.Matches(player2));
            Assert.IsTrue(player2.Matches(player1));
        }

        [TestMethod]
        public void CopyTo()
        {
            var player1 = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                ID = "jamesle01",
                Name = "LeBron James",
                FirstSeason = 2004,
                LastSeason = 2020,
                Position = "F-G",
                HeightInInches = 81,
                WeightInPounds = 250,
                BirthDate = new DateTime(1984, 12, 30).AsUtc(),
            };
            var player2 = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/l/",
                ID = "lamesje01",
                Name = "JeBron Lames",
                FirstSeason = 2020,
                LastSeason = 2036,
                Position = "C",
                HeightInInches = 90,
                WeightInPounds = 350,
                BirthDate = new DateTime(2004, 12, 30).AsUtc(),
            };

            Assert.IsFalse(player1.Matches(player2));

            player1.CopyTo(player2);

            Assert.IsTrue(player1.Matches(player2));
            Assert.AreEqual(81, player1.HeightInInches);
            Assert.AreEqual(81, player2.HeightInInches);
        }

        [TestMethod]
        public void GetProfileAndGameLogUrls()
        {
            var player = new Player
            {
                FeedUrl = "https://www.basketball-reference.com/players/j/",
                ID = "jamesle01",
                Name = "LeBron James",
                FirstSeason = 2004,
                LastSeason = 2020,
                Position = "F-G",
                HeightInInches = 81,
                WeightInPounds = 250,
                BirthDate = new DateTime(1984, 12, 30).AsUtc(),
            };

            Assert.AreEqual("https://www.basketball-reference.com/players/j/jamesle01.html", player.GetProfileUrl());
            Assert.AreEqual("https://www.basketball-reference.com/players/j/jamesle01/gamelog/2007/", player.GetGameLogUrl(2007));
        }
    }
}
