using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CovidCureIdApp.DataAccess;
using CovidCureIdApp.Model;
using Microsoft.Azure.WebJobs;
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
    public class DataProcessor
    {
        private RegimenRepository _regimens;

        /// <summary>
        ///     Injection constructor.
        /// </summary>
        /// <param name="regimens">Repository for interacting with regimens.</param>
        public DataProcessor(RegimenRepository regimens) {
            _regimens = regimens;
        }

        /// <summary>
        ///     Processes each JSON data file and parses the individual cases out into case files
        ///     in Cosmos.
        /// </summary>
        [FunctionName("ProcessDataFile")]
        public void ProcessDataFile(
            [BlobTrigger("covidcureid-raw-files/{name}", Connection = "AzureWebJobsStorage")]Stream blob, string name,
            [Queue("covidcureid-queue-drug")] ICollector<DrugEntry> drugEntryCollector,
            [Queue("covidcureid-queue-regimen")] ICollector<RegimenEntry> regimenEntryCollector,
            ILogger log)
        {
            log.LogInformation($"Processing file\n Name:{name} \n Size: {blob.Length} Bytes");

            int caseId = 0;

            try {
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
                    caseId = caseRoot.GetProperty("id").GetInt32();

                    log.LogInformation($"    Case: {caseId}");

                    // Create an entry for the primary drug associated with the case (using the case file)
                    DrugEntry drugEntry = DrugEntry.From(drugId, drugName, caseRoot);
                    drugEntryCollector.Add(drugEntry);

                    // Create an entry for the regimen
                    RegimenEntry regimenEntry = RegimenEntry.From(caseRoot);
                    regimenEntryCollector.Add(regimenEntry);
                }
            }
            catch(Exception exception) {
                log.LogError(exception, $"Error processing case ID: {caseId} in file: {name}");

                throw;
            }
        }

        /// <summary>
        ///     Processes a regimen entry placed on the queue.  The purpose of the queue is to throttle the writes to the database.
        /// </summary>
        [FunctionName("ProcessRegimenEntry")]
        public void ProcessRegimenEntry(
            [QueueTrigger("covidcureid-queue-regimen")] RegimenEntry entry,
            [CosmosDB(databaseName: "CovidCureId", collectionName: "CaseFiles", ConnectionStringSetting = "CovidCureIdCosmosDb")] out dynamic document,
            ILogger log)
        {
            // Check to see if there is already an entry for this regimen and do not duplicate the regimen entry
            Task<RegimenEntry> lookupTask = _regimens.Find(r => r.RegimenId == entry.RegimenId);
            lookupTask.Wait();

            RegimenEntry existingEntry = lookupTask.Result;

            if(existingEntry == null) {
                // Only set the entry if we don't already have an entry for this regimen.
                document = entry;
            }
            else {
                document = null;
            }
        }

        /// <summary>
        ///     Processes a drug entry placed on the queue.  The purpose of the queue is to throttle the writes to the database.
        /// </summary>
        [FunctionName("ProcessDrugEntry")]
        public void ProcessDrugEntry(
            [QueueTrigger("covidcureid-queue-drug")] DrugEntry entry,
            [CosmosDB(databaseName: "CovidCureId", collectionName: "CaseFiles", ConnectionStringSetting = "CovidCureIdCosmosDb")] out dynamic document,
            ILogger log)
        {
            document = entry;
        }
    }
}
