using CovidCureIdApp.DataAccess;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
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
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "query/{age}/{gender}")] HttpRequest req,
            int age,
            string gender,
            ILogger log)
        {
            // Find all of the drugs entries that match the age range and gender.
            QueryDefinition query = new QueryDefinition(@"
                SELECT
                    c.DrugName,
                    SUM(c.Improved) AS Improved,
                    SUM(c.Deteriorated) AS Deteriorated,
                    SUM(c.Undetermined) AS Undetermined
                FROM c
                WHERE c.EntryType = 'Drug'
                    AND @age >= c.AgeLowerBound
                    AND @age <= c.AgeUpperBound
                    AND LOWER(c.Gender) = @gender
                    GROUP BY c.DrugName")
                .WithParameter("@age", age)
                .WithParameter("@gender", gender.ToLowerInvariant());

            List<AggregateResult> results = await _drugs.Query<AggregateResult, DrugEntry>(query);

            return new OkObjectResult(results);
        }
    }
}