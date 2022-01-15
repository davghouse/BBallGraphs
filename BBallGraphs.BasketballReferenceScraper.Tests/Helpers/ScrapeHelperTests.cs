using BBallGraphs.BasketballReferenceScraper.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BBallGraphs.BasketballReferenceScraper.Tests.Helpers
{
    [TestClass]
    public class ScrapeHelperTests
    {
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
