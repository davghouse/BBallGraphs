using BBallGraphs.AzureStorage.BlobObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BBallGraphs.AzureStorage.Tests.BlobObjects
{
    [TestClass]
    public class PlayerBlobObjectTests
    {
        [TestMethod]
        public void Equals()
        {
            var player1 = new PlayerBlobObject
            {
                ID = "1",
                Name = "Test",
                FirstSeason = 2000,
                LastSeason = 2010,
            };

            var player2 = new PlayerBlobObject
            {
                ID = "1",
                Name = "Test",
                FirstSeason = 2000,
                LastSeason = 2010,
            };

            Assert.IsTrue(player1 != player2);
            Assert.IsTrue(player1.Equals(player2));

            player2.Name = "Test Test";
            Assert.IsFalse(player1.Equals(player2));
            Assert.IsFalse(player2.Equals(player1));

            player1.Name = "Test Test";
            Assert.IsTrue(player1.Equals(player2));
            Assert.IsTrue(player2.Equals(player1));

            player2 = null;
            Assert.IsFalse(player1.Equals(player2));
        }

        [TestMethod]
        public void CompareTo_1()
        {
            var brownde01 = new PlayerBlobObject
            {
                ID = "brownde01",
                Name = "Dee Brown"
            };

            var brownde02 = new PlayerBlobObject
            {
                ID = "brownde02",
                Name = "Devin Brown"
            };

            var brownde03 = new PlayerBlobObject
            {
                ID = "brownde03",
                Name = "Dee Brown"
            };

            Assert.IsTrue(brownde01.CompareTo(brownde02) < 0);
            Assert.IsTrue(brownde02.CompareTo(brownde01) > 0);
            Assert.IsTrue(brownde01.CompareTo(brownde01) == 0);
            Assert.IsTrue(brownde01.CompareTo(brownde03) < 0);
            Assert.IsTrue(brownde02.CompareTo(brownde03) > 0);

            Assert.IsTrue(new[] { brownde01, brownde03, brownde02 }.SequenceEqual(
                new[] { brownde01, brownde02, brownde03 }.OrderBy(p => p)));
        }

        [TestMethod]
        public void CompareTo_2()
        {
            var player1 = new PlayerBlobObject
            {
                ID = "aa01",
                Name = "A A A"
            };

            var player2 = new PlayerBlobObject
            {
                ID = "aab01",
                Name = "Ab A A"
            };

            Assert.IsTrue(player1.CompareTo(player2) < 0);
        }

        [TestMethod]
        public void CompareTo_3()
        {
            var players = new[]
            {
                new PlayerBlobObject
                {
                    ID = "burkmro01",
                    Name = "Roger Burkman"
                },
                new PlayerBlobObject
                {
                    ID = "burkepa01",
                    Name = "Pat Burke"
                },
                new PlayerBlobObject
                {
                    ID = "buechju01",
                    Name = "Jud Buechler"
                },
                new PlayerBlobObject
                {
                    ID = "burtode02",
                    Name = "Deonte Burton"
                },
                new PlayerBlobObject
                {
                    ID = "buntibi02",
                    Name = "Bill Bunting"
                },
                new PlayerBlobObject
                {
                    ID = "buckngr01",
                    Name = "Greg Buckner"
                },
                new PlayerBlobObject
                {
                    ID = "burdeti01",
                    Name = "Ticky Burden"
                },
                new PlayerBlobObject
                {
                    ID = "burketr01",
                    Name = "Trey Burke"
                },
                new PlayerBlobObject
                {
                    ID = "buntibi01",
                    Name = "Bill Buntin"
                }
            };

            Assert.IsTrue(new[] { players[5], players[2], players[8], players[4], players[6],
                players[1], players[7], players[0], players[3] }.SequenceEqual(
                players.OrderBy(p => p)));
        }
    }
}
