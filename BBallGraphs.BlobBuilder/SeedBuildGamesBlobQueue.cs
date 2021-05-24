using BBallGraphs.AzureStorage;
using BBallGraphs.AzureStorage.BlobObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBallGraphs.BlobBuilder
{
    public static class SeedBuildGamesBlobQueue
    {
        [FunctionName(nameof(SeedBuildGamesBlobQueue))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request,
            ILogger log)
        {
            var blobService = new BlobService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            string playersBlobContent = await blobService.DownloadPlayersBlobContent();
            var players = JsonConvert.DeserializeObject<IReadOnlyList<PlayerBlobObject>>(playersBlobContent ?? "[]");
            log.LogInformation($"Downloaded players blob: {players.Count} players found.");

            var queueService = new QueueService(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            foreach (var player in players) { await queueService.AddPlayerToBuildGamesBlobQueue(player.ID); }
            log.LogInformation($"Added {players.Count} players to BuildGamesBlob queue.");

            return new OkObjectResult($"Successfully seeded BuildGamesBlob queue with {players.Count} players.");
        }
    }
}
