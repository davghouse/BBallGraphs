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

        public async Task UploadBlobContent(string blobName, string content)
        {
            var blobReference = GetBlobReference(blobName);

            await blobReference.UploadTextAsync(content);
        }

        public async Task DeleteBlob(string blobName)
        {
            var blobReference = GetBlobReference(blobName);

            await blobReference.DeleteIfExistsAsync();
        }

        public CloudBlockBlob GetPlayersBlobReference()
            => GetBlobReference("players.json");

        public async Task<string> DownloadPlayersBlobContent()
            => await DownloadBlobContent("players.json");

        public async Task UploadPlayersBlobContent(string content)
            => await UploadBlobContent("players.json", content);
    }
}
