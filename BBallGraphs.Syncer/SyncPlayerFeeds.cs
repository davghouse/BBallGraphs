using BBallGraphs.AzureStorage;
using BBallGraphs.AzureStorage.SyncResults;
using BBallGraphs.BasketballReferenceScraper;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BBallGraphs.Syncer
{
    public static class SyncPlayerFeeds
    {
        [FunctionName(nameof(SyncPlayerFeeds))]
        public static async Task Run(
            // At 5:00 PM every day.
            [TimerTrigger("0 0 17 * * *")]TimerInfo timer,
            ILogger log)
        {
            var tableService = new TableService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var scraper = new Scraper(Environment.GetEnvironmentVariable("TransparentUserAgent"));

            var playerFeedRows = await tableService.GetPlayerFeedRows();
            log.LogInformation($"Queried player feeds table: {playerFeedRows.Count} rows returned.");

            var playerFeeds = await scraper.GetPlayerFeeds();
            log.LogInformation($"Scraped player feeds: {playerFeeds.Count} found.");

            var syncResult = new PlayerFeedsSyncResult(playerFeedRows, playerFeeds);

            if (syncResult.DefunctPlayerFeedRows.Any())
                throw new SyncException("Defunct player feed rows found, manual intervention required: " +
                    $"{string.Join(", ", syncResult.DefunctPlayerFeedRows.Select(r => r.Url))}");

            log.LogInformation(syncResult.NewPlayerFeedRows.Any()
                ? $"New player feeds found: {string.Join(", ", syncResult.NewPlayerFeedRows.Select(r => r.Url))}"
                : "No new player feeds found.");

            if (syncResult.FoundChanges)
            {
                await tableService.UpdatePlayerFeedsTable(syncResult);
                log.LogInformation("Player feeds table updated.");
            }
        }
    }
}
