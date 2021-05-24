using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace BBallGraphs.AzureStorage
{
    public class BlobService
    {
        private readonly CloudBlobContainer _container;

        public BlobService(string connectionString, string containerName = "bballgraphs")
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var blobClient = account.CreateCloudBlobClient();

            _container = blobClient.GetContainerReference(containerName);
        }

        public CloudBlockBlob GetBlobReference(string blobName)
            => _container.GetBlockBlobReference(blobName);

        public async Task<string> DownloadBlobContent(string blobName)
        {
            var blobReference = GetBlobReference(blobName);

            return await blobReference.ExistsAsync()
                ? await blobReference.DownloadTextAsync()
                : null;
        }

        public Task UploadBlobContent(string blobName, string content)
        {
            var blobReference = GetBlobReference(blobName);

            return blobReference.UploadTextAsync(content);
        }

        public Task DeleteBlob(string blobName)
        {
            var blobReference = GetBlobReference(blobName);

            return blobReference.DeleteIfExistsAsync();
        }

        public Task<string> DownloadPlayersBlobContent()
            => DownloadBlobContent("players.json");

        public Task UploadPlayersBlobContent(string content)
            => UploadBlobContent("players.json", content);

        public Task<string> DownloadGamesBlobContent(string playerID)
            => DownloadBlobContent($"{playerID}.json");

        public Task UploadGamesBlobContent(string playerID, string content)
            => UploadBlobContent($"{playerID}.json", content);
    }
}
