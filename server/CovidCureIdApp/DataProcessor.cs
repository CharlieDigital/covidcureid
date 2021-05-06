using System;
using System.IO;
using CovidCureIdApp.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CovidCureIdApp
{
    /// <summary>
    ///     This class contains the handlers for processing the input data files from Azure Storage
    /// </summary>
    /// <remarks>
    ///     See: https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/storage/Microsoft.Azure.WebJobs.Extensions.Storage.Queues#examples
    ///     for working with the Storage Queue bindings.
    /// </remarks>
    public static class DataProcessor
    {
        /// <summary>
        ///     Processes each JSON data file and parses the individual cases out into case files
        ///     in Cosmos.
        /// </summary>
        [FunctionName("ProcessDataFile")]
        public static void ProcessDataFile(
            [BlobTrigger("covidcureid-raw-files/{name}", Connection = "AzureWebJobsStorage")]Stream blob, string name,
            [Queue("covidcureid-queue-drug")] ICollector<DrugEntry> drugEntryCollector,
            [Queue("covidcureid-queue-regimen")] ICollector<RegimenEntry> regimenEntryCollector,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {blob.Length} Bytes");

        }

        /// <summary>
        ///     Processes a regimen entry placed on the queue.
        /// </summary>
        [FunctionName("ProcessRegimenEntry")]
        public static void ProcessRegimenEntry(
            [QueueTrigger("covidcureid-queue-regimen")] RegimenEntry entry,
            [CosmosDB(databaseName: "CovidCureId", collectionName: "CaseFiles", ConnectionStringSetting = "CovidCureIdCosmosDb")] out dynamic document,
            ILogger log)
        {
            document = new { id = Guid.NewGuid(), name = "Something", cureId = 1234 };
        }

        /// <summary>
        ///     Processes a drug entry placed on the queue.
        /// </summary>
        [FunctionName("ProcessDrugEntry")]
        public static void ProcessDrugEntry(
            [QueueTrigger("covidcureid-queue-drug")] DrugEntry entry,
            [CosmosDB(databaseName: "CovidCureId", collectionName: "CaseFiles", ConnectionStringSetting = "CovidCureIdCosmosDb")] out dynamic document,
            ILogger log)
        {
            document = new { id = Guid.NewGuid(), name = "Something", cureId = 1234 };
        }
    }
}
