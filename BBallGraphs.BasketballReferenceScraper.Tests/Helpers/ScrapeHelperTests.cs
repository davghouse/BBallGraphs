using BBallGraphs.BasketballReferenceScraper.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BBallGraphs.BasketballReferenceScraper.Tests.Helpers
{
    [TestClass]
    public class ScrapeHelperTests
    {
        [TestMethod]
        public void ParseHeightInInches()
        {
            Assert.AreEqual(72, ScrapeHelper.ParseHeightInInches("6-0"));
            Assert.AreEqual(84, ScrapeHelper.ParseHeightInInches("7-0"));
            Assert.AreEqual(75, ScrapeHelper.ParseHeightInInches("6-3"));
            Assert.AreEqual(85.5, ScrapeHelper.ParseHeightInInches("7-1.5"));
            Assert.AreEqual(60, ScrapeHelper.ParseHeightInInches("5 - 0"));
        }

        [TestMethod]
        public void ParseSecondsPlayed()
        {
            Assert.AreEqual(37*60, ScrapeHelper.ParseSecondsPlayed("37:00"));
            Assert.AreEqual(33*60 + 3, ScrapeHelper.ParseSecondsPlayed("33:03"));
            Assert.AreEqual(42*60 + 50, ScrapeHelper.ParseSecondsPlayed("42:50"));
            Assert.AreEqual(null, ScrapeHelper.ParseSecondsPlayed(null));
            Assert.AreEqual(null, ScrapeHelper.ParseSecondsPlayed(""));
            Assert.AreEqual(null, ScrapeHelper.ParseSecondsPlayed("  "));
        }

        [TestMethod]
        public void GetEstimatedBirthDate()
        {
            var birthDate = ScrapeHelper.GetEstimatedBirthDate("Bill Allen");

            Assert.AreEqual(1945, birthDate.Year);
            Assert.AreEqual(1, birthDate.Month);
            Assert.AreEqual(1, birthDate.Day);
            Assert.AreEqual(TimeSpan.Zero, birthDate.TimeOfDay);
            Assert.AreEqual(DateTimeKind.Utc, birthDate.Kind);
        }
    }
}
