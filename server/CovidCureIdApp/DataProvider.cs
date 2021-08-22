using CovidCureIdApp.DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CovidCureIdApp.Model;

namespace CovidCureIdApp
{
    /// <summary>
    ///     Functions which provide data to the front-end via APIs.
    /// </summary>
    public class DataProvider
    {
        private DrugRepository _drugs;
        private RegimenRepository _regimens;

        /// <summary>
        ///     Injection constructor.
        /// </summary>
        /// <param name="drugs">The injected repository instance for interfacing with drugs.</param>
        /// <param name="regimens">The injected repository instance for interfacing with regimens.</param>
        public DataProvider(DrugRepository drugs, RegimenRepository regimens) {
            _drugs = drugs;
            _regimens = regimens;
        }

        /// <summary>
        ///     Queries the drug entries based on the age and gender of the subject.
        /// </summary>
        /// <returns>A result which indicates the drug records matching the age and gender of the subject showing aggregate counts of improvement or deterioration.</returns>
        [FunctionName("QueryByDrugs")]
        public async Task<IActionResult> QueryByDrugs(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "query/drug/by/{age}/{gender}")] HttpRequest req,
            int age,
            string gender,
            ILogger log)
        {
            List<AggregateResult> results = await _drugs.GetDrugEntryByAgeAndGender(age, gender);

            return new OkObjectResult(results);
        }

        /// <summary>
        ///     Queries the regimen entries based on a given drug ID.
        /// </summary>
        /// <returns>A result set which contains the list of regimens corresponding to the given drug.</returns>
        [FunctionName("QueryByRegimen")]
        public async Task<IActionResult> QueryByRegimen(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "query/regimen/by/{age}/{gender}/{drugId}")] HttpRequest req,
            int age,
            string gender,
            int drugId,
            ILogger log)
        {
            List<RegimenResult> results = await _drugs.GetRegimenByDrugAgeAndGender(drugId, age, gender);

            return new OkObjectResult(results);
        }
    }
}