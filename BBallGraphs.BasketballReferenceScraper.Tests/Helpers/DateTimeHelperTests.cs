using BBallGraphs.BasketballReferenceScraper.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BBallGraphs.BasketballReferenceScraper.Tests.Helpers
{
    [TestClass]
    public class DateTimeHelperTests
    {
        [TestMethod]
        public void AsUtc()
        {
            var nowLocal = DateTime.Now;
            var nowUtc = nowLocal.ToUniversalTime();

            Assert.AreEqual(nowLocal.TimeOfDay, nowLocal.AsUtc().TimeOfDay);
            Assert.AreEqual(nowUtc.TimeOfDay, nowUtc.AsUtc().TimeOfDay);
            Assert.AreEqual(DateTimeKind.Utc, nowLocal.AsUtc().Kind);
        }
    }
}
