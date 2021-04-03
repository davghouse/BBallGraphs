using BBallGraphs.Scrapers.BasketballReference;
using BBallGraphs.Syncer.SyncResults;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace BBallGraphs.Syncer
{
    public static class SyncGames
    {
        [FunctionName(nameof(SyncGames))]
        public static async void Run(
            // Every 3 minutes, between 8:00 PM and 6:59 AM every day. Trying to go very easy on them.
            [TimerTrigger("0 */3 0-6,20-23 * * *")]TimerInfo timer,
            ILogger log)
        {
            var syncService = new AzureSyncService();

            var playerRow = (await syncService.GetNextPlayerRows(
                rowLimit: 1, minimumTimeSinceLastSync: TimeSpan.FromHours(12)))
                .SingleOrDefault();
            log.LogInformation("Queried players table for next player: " +
                $"{playerRow?.ToString() ?? "N/A"}.");
            if (playerRow == null) return;

            int syncSeason = playerRow.GetNextSyncSeason();

            var gameRows = await syncService.GetGameRows(playerRow, syncSeason);
            log.LogInformation($"Queried games table for the {syncSeason} season: {gameRows.Count} rows returned.");

            var games = await Scraper.GetGames(playerRow, syncSeason);
            log.LogInformation($"Scraped games for the {syncSeason} season: {games.Count} found.");

            var syncResult = new SyncGamesResult(gameRows, games);

            if (syncResult.DefunctGameRows.Any())
                throw new SyncException("Defunct game rows found, manual intervention required: " +
                    $"{string.Join(", ", syncResult.DefunctGameRows.Select(g => g.ID))}", playerRow.GetGameLogUrl(syncSeason));

            log.LogInformation(syncResult.NewGames.Any()
                ? $"New games found for the {syncSeason} season: {string.Join(", ", syncResult.NewGames.Select(g => g.ID))}"
                : $"No new games found for the {syncSeason} season.");
            log.LogInformation(syncResult.UpdatedGameRows.Any()
                ? $"Updated games found for the {syncSeason} season: {string.Join(", ", syncResult.UpdatedGameRows.Select(r => r.ID))}"
                : $"No updated games found for the {syncSeason} season.");

            if (syncResult.FoundChanges)
            {
                await syncService.UpdateGamesTable(syncResult);
                log.LogInformation("Games table updated.");
            }

            await syncService.RequeuePlayerRow(playerRow, syncSeason, syncResult.FoundChanges);
            log.LogInformation("Player row requeued.");
        }
    }
}
