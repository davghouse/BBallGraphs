using BBallGraphs.BasketballReferenceScraper.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BBallGraphs.BasketballReferenceScraper.Tests.Helpers
{
    [TestClass]
    public class ParseHelperTests
    {
        [TestMethod]
        public void ParseNullableInt()
        {
            Assert.AreEqual(3, ParseHelper.ParseNullableInt("3"));
            Assert.AreEqual(27, ParseHelper.ParseNullableInt(" 27 "));
            Assert.AreEqual(null, ParseHelper.ParseNullableInt(null));
            Assert.AreEqual(null, ParseHelper.ParseNullableInt(""));
            Assert.AreEqual(null, ParseHelper.ParseNullableInt(" "));
            Assert.ThrowsException<FormatException>(() => ParseHelper.ParseNullableInt("27.6"));

        }

        [TestMethod]
        public void ParseNullableDouble()
        {
            Assert.AreEqual(3, ParseHelper.ParseNullableDouble("3"));
            Assert.AreEqual(27, ParseHelper.ParseNullableDouble(" 27 "));
            Assert.AreEqual(27.6, ParseHelper.ParseNullableDouble("27.6"));
            Assert.AreEqual(null, ParseHelper.ParseNullableDouble(null));
            Assert.AreEqual(null, ParseHelper.ParseNullableDouble(""));
            Assert.AreEqual(null, ParseHelper.ParseNullableDouble(" "));
            Assert.ThrowsException<FormatException>(() => ParseHelper.ParseNullableDouble("seventeen and a half"));
        }

        [TestMethod]
        public void ParseNullableDateTime()
        {
            Assert.AreEqual(new DateTime(1990, 1, 1), ParseHelper.ParseNullableDateTime("January 1, 1990"));
            Assert.AreEqual(new DateTime(1990, 1, 1), ParseHelper.ParseNullableDateTime(" January 1, 1990 "));
            Assert.AreEqual(new DateTime(1999, 2, 20), ParseHelper.ParseNullableDateTime("February 20, 1999"));
            Assert.AreEqual(null, ParseHelper.ParseNullableDateTime(null));
            Assert.AreEqual(null, ParseHelper.ParseNullableDateTime(""));
            Assert.AreEqual(null, ParseHelper.ParseNullableDateTime(" "));
            Assert.ThrowsException<FormatException>(() => ParseHelper.ParseNullableDateTime("sometime in 2022"));
        }

        [TestMethod]
        public void ParseNullableHeightInInches()
        {
            Assert.AreEqual(72, ParseHelper.ParseNullableHeightInInches("6-0"));
            Assert.AreEqual(84, ParseHelper.ParseNullableHeightInInches("7-0"));
            Assert.AreEqual(75, ParseHelper.ParseNullableHeightInInches("6-3"));
            Assert.AreEqual(85.5, ParseHelper.ParseNullableHeightInInches("7-1.5"));
            Assert.AreEqual(60, ParseHelper.ParseNullableHeightInInches("5 - 0"));
            Assert.ThrowsException<FormatException>(() => ParseHelper.ParseNullableHeightInInches("6'7\""));
        }

        [TestMethod]
        public void ParseSecondsPlayed()
        {
            Assert.AreEqual(37*60, ParseHelper.ParseNullableSecondsPlayed("37:00"));
            Assert.AreEqual(33*60 + 3, ParseHelper.ParseNullableSecondsPlayed("33:03"));
            Assert.AreEqual(42*60 + 50, ParseHelper.ParseNullableSecondsPlayed("42:50"));
            Assert.AreEqual(null, ParseHelper.ParseNullableSecondsPlayed(null));
            Assert.AreEqual(null, ParseHelper.ParseNullableSecondsPlayed(""));
            Assert.AreEqual(null, ParseHelper.ParseNullableSecondsPlayed("  "));
            Assert.ThrowsException<FormatException>(() => ParseHelper.ParseNullableSecondsPlayed("17m13s"));
        }
    }
}
