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
    public static class SyncGames
    {
        [FunctionName(nameof(SyncGames))]
        public static async Task Run(
            // Every 3 minutes, between 7:00 PM and 7:59 AM every day except from 4 AM to 5 AM
            // when site maintenance seems like it might be happening. Trying to go very easy on them.
            [TimerTrigger("0 */3 0-3,5-7,19-23 * * *")]TimerInfo timer,
            ILogger log)
        {
            var tableService = new TableService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var playerRow = (await tableService.GetNextPlayerRows(
                rowLimit: 1, minimumTimeSinceLastSync: TimeSpan.FromHours(12)))
                .SingleOrDefault();
            log.LogInformation($"Queried players table for next player: {playerRow?.ToString() ?? "N/A"}.");
            if (playerRow == null) return;

            int syncSeason = playerRow.GetNextSyncSeason();

            var gameRows = await tableService.GetGameRows(playerRow, syncSeason);
            log.LogInformation($"Queried games table for {playerRow.Name}'s {syncSeason} season: {gameRows.Count} rows returned.");

            var scraper = new Scraper(Environment.GetEnvironmentVariable("TransparentUserAgent"));
            var games = await scraper.GetGames(playerRow, syncSeason);
            log.LogInformation($"Scraped games for {playerRow.Name}'s {syncSeason} season: {games.Count} found.");

            var syncResult = new GamesSyncResult(gameRows, games);

            if (syncResult.DefunctGameRows.Count() > gameRows.Count() / 2
                && !bool.Parse(Environment.GetEnvironmentVariable("AllowDefunctGameRowsMajority")))
                throw new SyncException("Defunct game rows majority found, manual intervention required: " +
                    $"{string.Join(", ", syncResult.DefunctGameRows.Select(r => r.ID))}", playerRow.GetGameLogUrl(syncSeason));

            // Games from the current season get updated relatively frequently. For safety, manually verify
            // some older updates. Except for plus-minus, which seems to be getting backfilled a bit right now.
            if (syncResult.UpdatedGameRows.Count(r => r.Season < DateTime.UtcNow.Year) > games.Count() / 2
                && (syncResult.UpdatedFields.Count > 1 || !syncResult.UpdatedFields.Contains(nameof(IGame.PlusMinus)))
                && !bool.Parse(Environment.GetEnvironmentVariable("AllowUpdatedHistoricalGameRowsMajority")))
                throw new SyncException($"Updated historical game rows majority found, manual intervention required: " +
                    $"{string.Join(", ", syncResult.UpdatedGameRows.Select(r => r.ID))}{Environment.NewLine}" +
                    $"Updated fields: {string.Join(", ", syncResult.UpdatedFields)}", playerRow.GetGameLogUrl(syncSeason));

            log.LogInformation(syncResult.DefunctGameRows.Any()
                ? $"Defunct games found for {playerRow.Name}'s {syncSeason} season: {string.Join(", ", syncResult.DefunctGameRows.Select(r => r.ID))}"
                : $"No defunct games found for {playerRow.Name}'s {syncSeason} season.");
            log.LogInformation(syncResult.NewGameRows.Any()
                ? $"New games found for {playerRow.Name}'s {syncSeason} season: {string.Join(", ", syncResult.NewGameRows.Select(r => r.ID))}"
                : $"No new games found for {playerRow.Name}'s {syncSeason} season.");
            log.LogInformation(syncResult.UpdatedGameRows.Any()
                ? $"Updated games found for {playerRow.Name}'s {syncSeason} season: {string.Join(", ", syncResult.UpdatedGameRows.Select(r => r.ID))}"
                : $"No updated games found for {playerRow.Name}'s {syncSeason} season.");

            if (syncResult.FoundChanges)
            {
                var queueService = new QueueService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
                await queueService.AddPlayerToBuildGamesBlobQueue(playerRow.ID);
                log.LogInformation($"Added {playerRow.Name} ({playerRow.ID}) to BuildGamesBlob queue.");

                await tableService.UpdateGamesTable(syncResult);
                log.LogInformation("Games table updated.");
            }

            await tableService.RequeuePlayerRow(playerRow, syncSeason, syncResult.FoundChanges);
            log.LogInformation("Player row requeued.");
        }
    }
}
