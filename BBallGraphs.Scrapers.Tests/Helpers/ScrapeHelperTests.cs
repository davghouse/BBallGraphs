using BBallGraphs.Scrapers.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BBallGraphs.Scrapers.Tests.Helpers
{
    [TestClass]
    public class ScrapeHelperTests
    {
        [TestMethod]
        public void ParseHeight()
        {
            Assert.AreEqual(72, ScrapeHelper.ParseHeight("6-0"));
            Assert.AreEqual(84, ScrapeHelper.ParseHeight("7-0"));
            Assert.AreEqual(75, ScrapeHelper.ParseHeight("6-3"));
            Assert.AreEqual(85.5, ScrapeHelper.ParseHeight("7-1.5"));
            Assert.AreEqual(60, ScrapeHelper.ParseHeight("5 - 0"));
        }

        [TestMethod]
        public void AsUtc()
        {
            var nowLocal = DateTime.Now;
            var nowUtc = nowLocal.ToUniversalTime();

            Assert.AreEqual(nowLocal.TimeOfDay, nowLocal.AsUtc().TimeOfDay);
            Assert.AreEqual(nowUtc.TimeOfDay, nowUtc.AsUtc().TimeOfDay);
            Assert.AreEqual(DateTimeKind.Utc, nowLocal.AsUtc().Kind);
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
