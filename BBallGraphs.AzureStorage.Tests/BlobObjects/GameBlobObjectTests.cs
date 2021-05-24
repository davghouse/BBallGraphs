using BBallGraphs.AzureStorage.BlobObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BBallGraphs.AzureStorage.Tests.BlobObjects
{
    [TestClass]
    public class GameBlobObjectTests
    {
        [TestMethod]
        public void Equals()
        {
            var game1 = new GameBlobObject
            {
                ID = "player1 2000/1/1",
                PlayerID = "player1",
                PlayerName = "Player 1",
                Season = 2000,
                Date = new DateTime(2000, 1, 1),
                Points = 15
            };

            var game2 = new GameBlobObject
            {
                ID = "player1 2000/1/1",
                PlayerID = "player1",
                PlayerName = "Player 1",
                Season = 2000,
                Date = new DateTime(2000, 1, 1),
                Points = 15
            };

            Assert.IsTrue(game1 != game2);
            Assert.IsTrue(game1.Equals(game2));

            game1.TotalRebounds = 10;
            Assert.IsFalse(game1.Equals(game2));

            game2.TotalRebounds = 10;
            Assert.IsTrue(game1.Equals(game2));
            Assert.IsTrue(game2.Equals(game1));

            game2.Date = new DateTime(2000, 1, 2);
            Assert.IsFalse(game1.Equals(game2));
            Assert.IsFalse(game2.Equals(game1));
        }
    }
}
