using BBallGraphs.AzureStorage.BlobObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BBallGraphs.AzureStorage.Tests
{
    [TestClass]
    public class BlobServiceTests
    {
        private BlobService _blobService;

        [TestInitialize]
        public void TestInitialize()
            => _blobService = new BlobService("UseDevelopmentStorage=true");

        [TestMethod]
        public async Task BlobCRUD()
        {
            string blobName = $"test{DateTime.UtcNow.Ticks}.json";

            string content = await _blobService.DownloadBlobContent(blobName);
            Assert.IsNull(content);

            await _blobService.UploadBlobContent(blobName, "test");
            content = await _blobService.DownloadBlobContent(blobName);
            Assert.AreEqual("test", content);

            await _blobService.UploadBlobContent(blobName, "test 2");
            content = await _blobService.DownloadBlobContent(blobName);
            Assert.AreEqual("test 2", content);

            await _blobService.DeleteBlob(blobName);
            content = await _blobService.DownloadBlobContent(blobName);
            Assert.IsNull(content);
        }

        [TestMethod]
        public async Task GamesBlobCRUD()
        {
            string playerID = $"testPlayer{DateTime.UtcNow.Ticks}";
            string content = await _blobService.DownloadGamesBlobContent(playerID);
            Assert.IsNull(content);

            var games = new GameBlobObject[0];
            await _blobService.UploadGamesBlobContent(playerID, JsonConvert.SerializeObject(games));
            content = await _blobService.DownloadGamesBlobContent(playerID);
            Assert.AreEqual("[]", content);

            games = new GameBlobObject[]
            {
                new GameBlobObject
                {
                    PlayerID = playerID,
                    PlayerName = "Test Player",
                    Season = 2000,
                    Date = new DateTime(2000, 1, 1),
                    Points = 5
                },
                new GameBlobObject
                {
                    PlayerID = playerID,
                    PlayerName = "Test Player",
                    Season = 2000,
                    Date = new DateTime(2000, 1, 2),
                    Points = 10
                }
            };

            await _blobService.UploadGamesBlobContent(playerID, JsonConvert.SerializeObject(games));
            content = await _blobService.DownloadGamesBlobContent(playerID);
            CollectionAssert.AreEqual(games, JsonConvert.DeserializeObject<GameBlobObject[]>(content));

            await _blobService.DeleteBlob($"{playerID}.json");
            content = await _blobService.DownloadGamesBlobContent(playerID);
            Assert.IsNull(content);
        }
    }
}
