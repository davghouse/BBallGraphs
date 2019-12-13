using BBallGraphs.Scrapers.BasketballReference;
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
                new PlayerFeed("https://www.basketball-reference.com/players/a/"));

            Assert.IsTrue(players.Count >= 166);
            Assert.AreEqual(players.Count, players.Select(p => p.Url).Distinct().Count());
            Assert.AreEqual(1, players.Count(p => p.Url.Contains("abdelal01")
                && p.Name == "Alaa Abdelnaby" && p.FromYear == 1991 && p.ToYear == 1995
                && p.HeightInches == 82 && p.BirthDate == new DateTime(1968, 6, 24)));
            Assert.AreEqual(1, players.Count(p => p.Url.Contains("azubuke01")
                && p.Name == "Kelenna Azubuike" && p.FromYear == 2007 && p.ToYear == 2012
                && p.HeightInches == 77 && p.BirthDate == new DateTime(1983, 12, 16)));
        }
    }
}
