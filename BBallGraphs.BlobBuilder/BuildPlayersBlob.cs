using BBallGraphs.AzureStorage;
using BBallGraphs.AzureStorage.BlobObjects;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallGraphs.BlobBuilder
{
    public static class BuildPlayersBlob
    {
        [FunctionName(nameof(BuildPlayersBlob))]
        public static async Task Run(
            // At 9:00 AM every day.
            [TimerTrigger("0 0 9 * * *")]TimerInfo timer,
            ILogger log)
        {
            var tableService = new TableService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var blobService = new BlobService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

            string playersBlobContent = await blobService.DownloadPlayersBlobContent();
            var blobPlayers = JsonConvert.DeserializeObject<IReadOnlyList<PlayerBlobObject>>(playersBlobContent ?? "[]");
            log.LogInformation($"Downloaded players blob: {blobPlayers.Count} players found.");

            var tablePlayers = (await tableService.GetPlayerRows())
                .Where(r => r.HasSyncedGames)
                .Select(r => new PlayerBlobObject(r))
                .OrderBy(p => p)
                .ToArray();
            log.LogInformation($"Queried players table: {tablePlayers.Length} players with synced games found.");

            if (!tablePlayers.SequenceEqual(blobPlayers))
            {
                await blobService.UploadPlayersBlobContent(JsonConvert.SerializeObject(tablePlayers));
                log.LogInformation("Players blob updated.");
            }
            else
            {
                log.LogInformation("Players blob already up to date.");
            }
        }
    }
}
