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
    public static class SyncPlayers
    {
        [FunctionName(nameof(SyncPlayers))]
        public static async Task Run(
            // Every 4 minutes, between 6:00 PM and 7:59 PM every day.
            [TimerTrigger("0 */4 18-19 * * *")]TimerInfo timer,
            ILogger log)
        {
            var tableService = new TableService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var playerFeedRow = (await tableService.GetNextPlayerFeedRows(
                rowLimit: 1, minimumTimeSinceLastSync: TimeSpan.FromHours(12)))
                .SingleOrDefault();
            log.LogInformation($"Queried player feeds table for next feed: {playerFeedRow?.ToString() ?? "N/A"}.");
            if (playerFeedRow == null) return;

            var playerRows = await tableService.GetPlayerRows(playerFeedRow);
            log.LogInformation($"Queried players table: {playerRows.Count} rows returned.");

            var scraper = new Scraper(Environment.GetEnvironmentVariable("TransparentUserAgent"));
            var players = await scraper.GetPlayers(playerFeedRow);
            log.LogInformation($"Scraped players: {players.Count} found.");

            var syncResult = new PlayersSyncResult(playerRows, players);

            if (syncResult.DefunctPlayerRows.Any()
                && !bool.Parse(Environment.GetEnvironmentVariable("AllowDefunctPlayerRows")))
                throw new SyncException("Defunct player rows found, manual intervention required: " +
                    $"{string.Join(", ", syncResult.DefunctPlayerRows.Select(r => r.ID))}", playerFeedRow.Url);

            log.LogInformation(syncResult.DefunctPlayerRows.Any()
                ? $"Defunct players found: {string.Join(", ", syncResult.DefunctPlayerRows.Select(r => r.ID))}"
                : "No defunct players found.");
            log.LogInformation(syncResult.NewPlayerRows.Any()
                ? $"New players found: {string.Join(", ", syncResult.NewPlayerRows.Select(r => r.ID))}"
                : "No new players found.");
            log.LogInformation(syncResult.UpdatedPlayerRows.Any()
                ? $"Updated players found: {string.Join(", ", syncResult.UpdatedPlayerRows.Select(r => r.ID))}"
                : "No updated players found.");

            if (syncResult.FoundChanges)
            {
                await tableService.UpdatePlayersTable(syncResult);
                log.LogInformation("Players table updated.");
            }

            await tableService.RequeuePlayerFeedRow(playerFeedRow, syncResult.FoundChanges);
            log.LogInformation("Player feed row requeued.");
        }
    }
}
