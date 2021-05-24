using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading;
using System.Threading.Tasks;

namespace BBallGraphs.AzureStorage.Tests
{
    [TestClass]
    public class QueueServiceTests
    {
        private static int _queueNameDeduplicator = 0;
        private CloudQueue _buildGamesBlobQueue;
        private QueueService _queueService;

        [TestInitialize]
        public async Task TestInitialize()
        {
            var account = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var queueClient = account.CreateCloudQueueClient();

            _buildGamesBlobQueue = queueClient.GetQueueReference(
                $"bballgraphsbuildgamesblob{Interlocked.Increment(ref _queueNameDeduplicator)}");
            await _buildGamesBlobQueue.CreateIfNotExistsAsync();

            _queueService = new QueueService(_buildGamesBlobQueue);
        }

        [TestCleanup]
        public async Task TestCleanup()
            => await _buildGamesBlobQueue.DeleteAsync();

        [TestMethod]
        public async Task BuildGamesBlobQueue()
        {
            string playerID = await _queueService.GetNextPlayerFromBuildGamesBlobQueue();
            Assert.AreEqual(null, playerID);

            await _queueService.AddPlayerToBuildGamesBlobQueue("testPlayer1");
            playerID = await _queueService.GetNextPlayerFromBuildGamesBlobQueue();
            Assert.AreEqual("testPlayer1", playerID);

            await _queueService.AddPlayerToBuildGamesBlobQueue("testPlayer2");
            playerID = await _queueService.GetNextPlayerFromBuildGamesBlobQueue();
            Assert.AreEqual("testPlayer1", playerID);

            await _queueService.RemovePlayerFromBuildGamesBlobQueue("testPlayer1");
            playerID = await _queueService.GetNextPlayerFromBuildGamesBlobQueue();
            Assert.AreEqual("testPlayer2", playerID);

            await _queueService.RemovePlayerFromBuildGamesBlobQueue("testPlayer2");
            playerID = await _queueService.GetNextPlayerFromBuildGamesBlobQueue();
            Assert.AreEqual(null, playerID);
        }
    }
}
