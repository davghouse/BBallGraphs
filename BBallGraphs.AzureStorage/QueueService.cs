using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading.Tasks;

namespace BBallGraphs.AzureStorage
{
    public class QueueService
    {
        private readonly CloudQueue _buildGamesBlobQueue;

        public QueueService(string connectionString,
            string buildGamesBlobQueueName = "bballgraphsbuildgamesblob")
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var queueClient = account.CreateCloudQueueClient();

            _buildGamesBlobQueue = queueClient.GetQueueReference(buildGamesBlobQueueName);
        }

        public QueueService(CloudQueue buildGamesBlobQueue)
            => _buildGamesBlobQueue = buildGamesBlobQueue;

        public Task AddPlayerToBuildGamesBlobQueue(string playerID)
            => _buildGamesBlobQueue.AddMessageAsync(new CloudQueueMessage(playerID));

        public async Task<string> GetNextPlayerFromBuildGamesBlobQueue()
            => (await _buildGamesBlobQueue.PeekMessageAsync())?.AsString;

        public async Task RemovePlayerFromBuildGamesBlobQueue(string playerID)
        {
            var message = await _buildGamesBlobQueue.GetMessageAsync();
            if (playerID != message.AsString)
                throw new NotSupportedException($"{playerID} is no longer at the start of the queue.");

            await _buildGamesBlobQueue.DeleteMessageAsync(message);
        }
    }
}
