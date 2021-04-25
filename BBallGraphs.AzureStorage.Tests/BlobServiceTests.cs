using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
