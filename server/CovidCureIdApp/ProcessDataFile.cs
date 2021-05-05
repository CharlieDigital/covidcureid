using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CovidCureIdApp
{
    /// <summary>
    ///     This Function responds to JSON file uploads and processes each file by inserting
    ///     records into CosmosDB representing each case.
    /// </summary>
    public static class ProcessDataFile
    {
        /// <summary>
        ///     Processes each JSON data file and parses the individual cases out into case files
        ///     in Cosmos.
        /// </summary>
        [FunctionName("ProcessDataFile")]
        public static void Run(
            [BlobTrigger("covidcureid-raw-files/{name}", Connection = "AzureWebJobsStorage")]Stream blob, string name,
            [CosmosDB(databaseName: "CovidCureId", collectionName: "CaseFiles", ConnectionStringSetting = "CovidCureIdCosmosDb")] out dynamic document,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {blob.Length} Bytes");

            document = new { id = Guid.NewGuid(), name = "Something", cureId = 1234 };
        }
    }
}
