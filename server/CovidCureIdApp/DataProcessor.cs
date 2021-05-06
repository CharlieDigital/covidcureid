using System;
using System.IO;
using System.Text.Json;
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
            log.LogInformation($"Processing file\n Name:{name} \n Size: {blob.Length} Bytes");

            string[] nameParts = name.Split('-', 3);
            int drugId = Convert.ToInt32(nameParts[1]);
            string drugName = nameParts[2].Replace(".json", string.Empty);

            JsonDocument json = JsonDocument.Parse(blob);
            JsonElement root = json.RootElement;

            int caseCount = root.GetArrayLength();

            log.LogInformation($"  Drug: {drugName} ({drugId}) has {caseCount} cases");

            // Process each case.
            for(int i = 0; i < caseCount; i++) {
                JsonElement caseRoot = root[i];
                int caseId = caseRoot.GetProperty("id").GetInt32();

                log.LogInformation($"    Case: {caseId}");

                // Create an entry for the primary drug associated with the case (using the case file)
                DrugEntry drugEntry = DrugEntry.From(drugId, drugName, caseRoot);

                log.LogInformation($"      {JsonSerializer.Serialize(drugEntry, drugEntry.GetType(), new JsonSerializerOptions { WriteIndented = true })}");
                log.LogInformation($"        {drugEntry.AgeLowerBound} - {drugEntry.AgeUpperBound}");

                // Create an entry for the regimen
            }
        }

        /// <summary>
        ///     Processes a regimen entry placed on the queue.  The purpose of the queue is to throttle the writes to the database.
        /// </summary>
        [FunctionName("ProcessRegimenEntry")]
        public static void ProcessRegimenEntry(
            [QueueTrigger("covidcureid-queue-regimen")] RegimenEntry entry,
            [CosmosDB(databaseName: "CovidCureId", collectionName: "CaseFiles", ConnectionStringSetting = "CovidCureIdCosmosDb")] out dynamic document,
            ILogger log)
        {
            // Check to see if there is already an entry for this regimen and do not duplicate the regimen entry

            document = entry;
        }

        /// <summary>
        ///     Processes a drug entry placed on the queue.  The purpose of the queue is o throttle the writes to the database.
        /// </summary>
        [FunctionName("ProcessDrugEntry")]
        public static void ProcessDrugEntry(
            [QueueTrigger("covidcureid-queue-drug")] DrugEntry entry,
            [CosmosDB(databaseName: "CovidCureId", collectionName: "CaseFiles", ConnectionStringSetting = "CovidCureIdCosmosDb")] out dynamic document,
            ILogger log)
        {
            document = entry;
        }
    }
}
