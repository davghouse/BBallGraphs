﻿using BBallGraphs.AzureStorage;
using BBallGraphs.AzureStorage.BlobObjects;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BBallGraphs.BlobBuilder
{
    public static class BuildGamesBlob
    {
        [FunctionName(nameof(BuildGamesBlob))]
        public static async Task Run(
            // At 20 seconds past the minute every 3 minutes, between 7:00 PM and 7:59 AM every day except
            // from 4 AM to 5 AM when site maintenance seems like it might be happening--matching SyncGames.
            [TimerTrigger("20 */3 0-3,5-7,19-23 * * *")]TimerInfo timer,
            ILogger log)
        {
            var queueService = new QueueService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            string playerID = await queueService.GetNextPlayerFromBuildGamesBlobQueue();
            log.LogInformation($"Got next player from BuildGamesBlob queue: {playerID ?? "N/A"}.");
            if (playerID == null) return;

            var tableService = new TableService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var games = (await tableService.GetGameRows(playerID))
                .Select(r => new GameBlobObject(r))
                .OrderBy(r => r.Date)
                .ToArray();
            log.LogInformation($"Queried games table for {playerID}: {games.Length} games found.");

            var blobService = new BlobService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            await blobService.UploadGamesBlobContent(playerID, JsonConvert.SerializeObject(games));
            log.LogInformation($"{playerID}'s games blob updated.");

            await queueService.RemovePlayerFromBuildGamesBlobQueue(playerID);
            log.LogInformation($"Removed {playerID} from BuildGamesBlob queue.");
        }
    }
}
