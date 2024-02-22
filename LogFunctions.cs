using System;
using System.Linq;
using System.Threading.Tasks;
using FetchingDataTimerFunction.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace FetchingDataTimerFunction
{
   public static class LogFunctions
   {
      private static readonly CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

      [FunctionName("GetLogs")]
      public static async Task<IActionResult> GetLogs(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "logs")] HttpRequest req,
          ILogger log)
      {
         log.LogInformation("Fetching logs based on query parameters.");
         var query = req.Query;
         var from = DateTimeOffset.Parse(query["from"]);
         var to = DateTimeOffset.Parse(query["to"]);

         var tableClient = storageAccount.CreateCloudTableClient();
         var table = tableClient.GetTableReference("log");

         var queryOperation = new TableQuery<LogEntity>().Where(
             TableQuery.CombineFilters(
                 TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, from),
                 TableOperators.And,
                 TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, to)));

         var segment = await table.ExecuteQuerySegmentedAsync(queryOperation, null);
         var logs = segment.Select(x => new LogModel { Timestamp = x.Timestamp, Success = x.Success, LogId = x.RowKey }).ToList();

         log.LogInformation($"Retrieved {logs.Count} logs.");
         return new OkObjectResult(logs);
      }

      [FunctionName("GetPayload")]
      public static async Task<IActionResult> GetPayload(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "payload/{logId}")] HttpRequest req,
          string logId,
          ILogger log)
      {
         log.LogInformation($"Fetching payload for logId: {logId}");
         var blobClient = storageAccount.CreateCloudBlobClient();
         var container = blobClient.GetContainerReference("payloads");

         var blob = container.GetBlockBlobReference(logId);
         if (!await blob.ExistsAsync())
         {
            log.LogWarning($"Payload not found for logId: {logId}");
            return new NotFoundResult();
         }

         var payload = await blob.DownloadTextAsync();
         log.LogInformation($"Payload retrieved successfully for logId: {logId}");
         return new OkObjectResult(payload);
      }
   }
}
