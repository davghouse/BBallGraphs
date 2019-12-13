using BBallGraphs.Scrapers.BasketballReference;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace BBallGraphs.Syncer
{
    public static class SyncPlayerFeeds
    {
        [FunctionName(nameof(SyncPlayerFeeds))]
        public static async Task Run(
            [TimerTrigger("0 0 3 * * *")]TimerInfo timer, // Every day @ 3:00 AM
            ILogger log)
        {
            var playerFeeds = (await Scraper.GetPlayerFeeds())
                .ToDictionary(f => f.Url);
            log.LogInformation($"Scraped player feeds: {playerFeeds.Count} found.");

            var syncService = new AzureSyncService();
            var playerFeedRows = (await syncService.GetPlayerFeedRows())
                .ToDictionary(f => f.Url);
            log.LogInformation($"Queried player feeds table: {playerFeedRows.Count} rows returned.");

            // We don't expect feeds to disappear. If we have some feeds in the table that are no
            // longer found when scraping, we require a code change and reassessment of assumptions.
            var defunctPlayerFeedRows = playerFeedRows.Values
                .Where(r => !playerFeeds.ContainsKey(r.Url))
                .ToArray();
            if (defunctPlayerFeedRows.Any())
                throw new SyncException("Defunct player feed rows found: " +
                    $"{string.Join(", ", defunctPlayerFeedRows.Select(r => r.Url))}");

            var newPlayerFeeds = playerFeeds.Values
                .Where(f => !playerFeedRows.ContainsKey(f.Url))
                .ToArray();
            if (newPlayerFeeds.Any())
            {
                log.LogInformation("New player feeds found: " +
                    $"{string.Join(", ", newPlayerFeeds.Select(f => f.Url))}");

                await syncService.InsertPlayerFeedRows(newPlayerFeeds);
                log.LogInformation("New player feeds inserted.");
            }
            else
            {
                log.LogInformation("No changes to the feeds found.");
            }
        }
    }
}
