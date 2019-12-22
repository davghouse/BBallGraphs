using BBallGraphs.Scrapers.BasketballReference;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BBallGraphs.Scrapers.Tests.BasketballReference
{
    [TestClass]
    public class IPlayerFeedTests
    {
        [TestMethod]
        public void Matches()
        {
            var playerFeed1 = new PlayerFeed { Url = "1" };
            var playerFeed2 = new PlayerFeed { Url = "2" };

            Assert.IsTrue(playerFeed1.Matches(playerFeed1));
            Assert.IsFalse(playerFeed1.Matches(playerFeed2));
            Assert.IsFalse(playerFeed2.Matches(playerFeed1));

            playerFeed2.Url = "1";

            Assert.IsTrue(playerFeed1.Matches(playerFeed2));
            Assert.IsTrue(playerFeed2.Matches(playerFeed1));
        }

        [TestMethod]
        public void CopyTo()
        {
            var playerFeed1 = new PlayerFeed { Url = "1" };
            var playerFeed2 = new PlayerFeed { Url = "2" };

            Assert.IsFalse(playerFeed1.Matches(playerFeed2));

            playerFeed1.CopyTo(playerFeed2);

            Assert.IsTrue(playerFeed1.Matches(playerFeed2));
        }
    }
}
