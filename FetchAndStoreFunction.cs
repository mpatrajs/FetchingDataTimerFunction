using System;
using System.Net.Http;
using System.Threading.Tasks;
using FetchingDataTimerFunction.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace FetchingDataTimerFunction
{
   public static class FetchAndStoreFunction
   {
      private static readonly HttpClient httpClient = new();
      private static readonly CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

      [FunctionName("FetchAndStoreFunction")]
      public static async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
      {
         var apiUrl = Environment.GetEnvironmentVariable("API_URL");
         log.LogInformation($"Fetching data from API: {apiUrl}");

         try
         {
            var response = await httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
               var json = await response.Content.ReadAsStringAsync();
               var logEntity = new LogEntity { PartitionKey = "Logs", RowKey = Guid.NewGuid().ToString(), Payload = json };

               log.LogInformation("Successfully fetched data from API");
               await StorePayloadInBlob(logEntity.RowKey, json, log);
               await LogAttempt(logEntity, true, log);
            }
            else
            {
               log.LogWarning($"Failed to fetch data from API: {apiUrl} with StatusCode: {response.StatusCode}");
               await LogAttempt(new LogEntity { PartitionKey = "Logs", RowKey = Guid.NewGuid().ToString() }, false, log);
            }
         }
         catch (Exception ex)
         {
            log.LogError(ex, "An error occurred while fetching and storing data.");
            await LogAttempt(new LogEntity { PartitionKey = "Logs", RowKey = Guid.NewGuid().ToString() }, false, log);
         }
      }

      private static async Task StorePayloadInBlob(string blobName, string payload, ILogger log)
      {
         try
         {
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("payloads");
            await container.CreateIfNotExistsAsync();

            var blob = container.GetBlockBlobReference(blobName);
            await blob.UploadTextAsync(payload);
            log.LogInformation($"Payload successfully stored in Blob: {blobName}");
         }
         catch (Exception ex)
         {
            log.LogError($"Error storing payload in Blob: {blobName}. Exception: {ex.Message}");
         }
      }


      private static async Task LogAttempt(LogEntity entity, bool isSuccess, ILogger log)
      {
         try
         {
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("log");
            await table.CreateIfNotExistsAsync();

            entity.Timestamp = DateTimeOffset.UtcNow;
            entity.Success = isSuccess;

            var insertOperation = TableOperation.Insert(entity);
            await table.ExecuteAsync(insertOperation);
            log.LogInformation($"Log attempt for entity: {entity.RowKey}, Success: {isSuccess}");
         }
         catch (Exception ex)
         {
            log.LogError($"Error logging attempt for entity: {entity.RowKey}, Success: {isSuccess}. Exception: {ex.Message}");
         }
      }

   }
}
