using BBallGraphs.Scrapers.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BBallGraphs.Scrapers.Tests.Helpers
{
    [TestClass]
    public class IEnumerableExtensionsTests
    {
        [TestMethod]
        public void NullableSum()
        {
            int? a = null;
            int? b = 3;
            var ca = new { v = a };
            var cb = new { v = b };

            Assert.AreEqual(3, new[] { a, b }.NullableSum());
            Assert.AreEqual(3, new[] { b }.NullableSum());
            Assert.AreEqual(null, new[] { a }.NullableSum());
            Assert.AreEqual(null, new int?[] { }.NullableSum());

            Assert.AreEqual(3, new[] { ca, cb }.NullableSum(c => c.v));
            Assert.AreEqual(3, new[] { cb }.NullableSum(c => c.v));
            Assert.AreEqual(null, new[] { ca }.NullableSum(c => c.v));
        }
    }
}
