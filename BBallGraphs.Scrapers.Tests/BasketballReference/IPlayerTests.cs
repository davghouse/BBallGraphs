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
                Url = "https://www.basketball-reference.com/players/j/jamesle01.html",
                Name = "LeBron James",
                FromYear = 2004,
                ToYear = 2020,
                Position = "F-G",
                HeightInches = 81,
                WeightPounds = 250,
                BirthDate = new DateTime(1984, 12, 30).AsUtc(),
                FeedUrl = "https://www.basketball-reference.com/players/j/"
            };
            var player2 = new Player
            {
                Url = "https://www.basketball-reference.com/players/l/lamesje01.html",
                Name = "JeBron Lames",
                FromYear = 2020,
                ToYear = 2036,
                Position = "C",
                HeightInches = 90,
                WeightPounds = 350,
                BirthDate = new DateTime(2004, 12, 30).AsUtc(),
                FeedUrl = "https://www.basketball-reference.com/players/l/"
            };

            Assert.IsTrue(player1.Matches(player1));
            Assert.IsFalse(player1.Matches(player2));
            Assert.IsFalse(player2.Matches(player1));

            player2.Url = "https://www.basketball-reference.com/players/j/jamesle01.html";
            player2.Name = "LeBron James";
            player2.FromYear = 2004;
            player2.Position = "F-G";
            player2.HeightInches = 81;
            player2.BirthDate = new DateTime(1984, 12, 30).AsUtc();
            player2.FeedUrl = "https://www.basketball-reference.com/players/j/";

            Assert.IsFalse(player1.Matches(player2));
            Assert.IsFalse(player2.Matches(player1));

            player2.ToYear = 2020;
            player2.WeightPounds = 250;

            Assert.IsTrue(player1.Matches(player2));
            Assert.IsTrue(player2.Matches(player1));
        }

        [TestMethod]
        public void CopyTo()
        {
            var player1 = new Player
            {
                Url = "https://www.basketball-reference.com/players/j/jamesle01.html",
                Name = "LeBron James",
                FromYear = 2004,
                ToYear = 2020,
                Position = "F-G",
                HeightInches = 81,
                WeightPounds = 250,
                BirthDate = new DateTime(1984, 12, 30).AsUtc(),
                FeedUrl = "https://www.basketball-reference.com/players/j/"
            };
            var player2 = new Player
            {
                Url = "https://www.basketball-reference.com/players/l/lamesje01.html",
                Name = "JeBron Lames",
                FromYear = 2020,
                ToYear = 2036,
                Position = "C",
                HeightInches = 90,
                WeightPounds = 350,
                BirthDate = new DateTime(2004, 12, 30).AsUtc(),
                FeedUrl = "https://www.basketball-reference.com/players/l/"
            };

            Assert.IsFalse(player1.Matches(player2));

            player1.CopyTo(player2);

            Assert.IsTrue(player1.Matches(player2));
        }
    }
}
