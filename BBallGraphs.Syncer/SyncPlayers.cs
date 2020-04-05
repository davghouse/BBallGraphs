using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Syncer.SyncResults;
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
            // Every 4 minutes, between 06:00 PM and 07:59 PM every day.
            [TimerTrigger("0 */4 18-19 * * *")]TimerInfo timer,
            ILogger log)
        {
            var syncService = new AzureSyncService();

            var playerFeedRow = (await syncService.GetNextPlayerFeedRows(
                rowLimit: 1, minimumTimeSinceLastSync: TimeSpan.FromHours(12)))
                .SingleOrDefault();
            log.LogInformation("Queried player feeds table for next feed: " +
                $"{playerFeedRow?.Url ?? "N/A"}.");
            if (playerFeedRow == null) return;

            var playerRows = await syncService.GetPlayerRows(playerFeedRow);
            log.LogInformation($"Queried players table: {playerRows.Count} rows returned.");

            var players = await Scraper.GetPlayers(playerFeedRow);
            log.LogInformation($"Scraped players: {players.Count} found.");

            var syncResult = new SyncPlayersResult(playerRows, players);

            // We don't expect players to disappear from feeds. It can happen legitimately (like
            // Ron Artest moving to /w/), but let's manually verify each occurrence like that.
            if (syncResult.DefunctPlayerRows.Any())
                throw new SyncException("Defunct player rows found, manual intervention required: " +
                    $"{string.Join(", ", syncResult.DefunctPlayerRows.Select(r => r.ID))}", playerFeedRow.Url);

            log.LogInformation(syncResult.NewPlayers.Any()
                ? $"New players found: {string.Join(", ", syncResult.NewPlayers.Select(p => p.ID))}"
                : "No new players found.");
            log.LogInformation(syncResult.UpdatedPlayerRows.Any()
                ? $"Updated players found: {string.Join(", ", syncResult.UpdatedPlayerRows.Select(r => r.ID))}"
                : "No updated players found.");

            if (syncResult.FoundChanges)
            {
                await syncService.UpdatePlayersTable(syncResult);
                log.LogInformation("Players table updated.");
            }

            await syncService.RequeuePlayerFeedRow(playerFeedRow, syncResult.FoundChanges);
            log.LogInformation("Player feed row requeued.");
        }
    }
}
