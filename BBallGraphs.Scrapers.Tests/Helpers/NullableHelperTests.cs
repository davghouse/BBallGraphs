using BBallGraphs.Scrapers.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BBallGraphs.Scrapers.Tests.Helpers
{
    [TestClass]
    public class NullableHelperTests
    {
        [TestMethod]
        public void TryParseInt()
        {
            Assert.AreEqual(3, NullableHelper.TryParseInt("3"));
            Assert.AreEqual(27, NullableHelper.TryParseInt(" 27 "));
            Assert.AreEqual(null, NullableHelper.TryParseInt("27.6"));
            Assert.AreEqual(null, NullableHelper.TryParseInt(null));
            Assert.AreEqual(null, NullableHelper.TryParseInt(""));
            Assert.AreEqual(null, NullableHelper.TryParseInt(" "));
        }

        [TestMethod]
        public void TryParseDouble()
        {
            Assert.AreEqual(3, NullableHelper.TryParseDouble("3"));
            Assert.AreEqual(27, NullableHelper.TryParseDouble(" 27 "));
            Assert.AreEqual(27.6, NullableHelper.TryParseDouble("27.6"));
            Assert.AreEqual(null, NullableHelper.TryParseDouble(null));
            Assert.AreEqual(null, NullableHelper.TryParseDouble(""));
            Assert.AreEqual(null, NullableHelper.TryParseDouble(" "));
        }
    }
}
