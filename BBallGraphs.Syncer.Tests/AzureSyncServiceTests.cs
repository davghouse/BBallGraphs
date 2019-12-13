using BBallGraphs.Scrapers.BasketballReference;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BBallGraphs.Syncer.Tests
{
    [TestClass]
    public class AzureSyncServiceTests
    {
        private static int _tableNameDeduplicator = 0;
        private CloudTable _playerFeedsTable;
        private AzureSyncService _syncService;

        [TestInitialize]
        public async Task TestInitialize()
        {
            var account = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var tableClient = account.CreateCloudTableClient();

            _playerFeedsTable = tableClient.GetTableReference(
                $"PlayerFeeds{Interlocked.Increment(ref _tableNameDeduplicator)}");
            await _playerFeedsTable.CreateIfNotExistsAsync();

            _syncService = new AzureSyncService(_playerFeedsTable);
        }

        [TestCleanup]
        public async Task TestCleanup()
            => await _playerFeedsTable.DeleteAsync();

        [TestMethod]
        public async Task InsertAndGetPlayerFeeds()
        {
            var playerFeeds = Enumerable.Range(1, 50)
                .Select(i => new PlayerFeed($"https://www.basketball-reference.com/players/{i}/"));
            await _syncService.InsertPlayerFeedRows(playerFeeds);
            var playerFeedRows = await _syncService.GetPlayerFeedRows();

            CollectionAssert.AreEquivalent(
                playerFeeds.Select(f => f.Url).ToArray(),
                playerFeedRows.Select(r => r.Url).ToArray());
            Assert.IsTrue(playerFeedRows.All(r => r.PartitionKey == "0"));
            Assert.IsTrue(playerFeedRows.All(r => !r.LastSyncedTimeUtc.HasValue));
        }
    }
}
