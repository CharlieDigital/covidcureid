using System.Collections.Generic;
using System.Threading.Tasks;
using CovidCureIdApp.Model;
using Microsoft.Azure.Cosmos;

namespace CovidCureIdApp.DataAccess
{
    /// <summary>
    ///     Repository class for interacting with drug entries.
    /// </summary>
    public class DrugRepository : CosmosRepositoryBase<DrugEntry>
    {
        /// <summary>
        ///     Injection constructor.
        /// </summary>
        /// <param name="cosmos">The instance of the gateway used for connecting to Cosmos</param>
        /// <returns>An instance of the repository.</returns>
        public DrugRepository(CosmosGateway cosmos) : base(cosmos)
        {
        }

        /// <summary>
        ///     Retrieves the list of aggregate drug entry results filtered by the age and
        ///     gender of the patient treated with the drug.  This result is displayed as the
        ///     stacked bar chart in the UI.
        /// </summary>
        /// <param name="age">The age of the patient treated with the drug.</param>
        /// <param name="gender">The gender of the patient.</param>
        /// <returns>A list of results indicating whether patients improved, deteriorated, or had an undetermined outcome.</returns>
        public async Task<List<AggregateResult>> GetDrugEntryByAgeAndGender(int age, string gender)
        {
            // Find all of the drugs entries that match the age range and gender.
            QueryDefinition query = new QueryDefinition(@"
                SELECT
                    c.DrugName,
                    c.DrugId,
                    SUM(c.Improved) AS Improved,
                    SUM(c.Deteriorated) AS Deteriorated,
                    SUM(c.Undetermined) AS Undetermined
                FROM c
                WHERE c.EntryType = 'Drug'
                    AND @age >= c.AgeLowerBound
                    AND @age <= c.AgeUpperBound
                    AND LOWER(c.Gender) = @gender
                    GROUP BY c.DrugName, c.DrugId")
                .WithParameter("@age", age)
                .WithParameter("@gender", gender.ToLowerInvariant());

            List<AggregateResult> results = await Query<AggregateResult, DrugEntry>(query);

            return results;
        }

        /// <summary>
        ///     Retrieves the list of regimens for cases for a given drug filtered by age
        ///     and gender.  This result is displayed on the right panel when a particular
        ///     drug is clicked on the stacked bar chart.
        /// </summary>
        /// <param name="drugId">The ID of a drug that is part of the regimen</param>
        /// <param name="age">The age of the patient treated with the regimen.</param>
        /// <param name="gender">The gender of the patient.</param>
        /// <returns>A list of regimens that match contain the drug as part of the treatment.</returns>
        public async Task<List<RegimenResult>> GetRegimenByDrugAgeAndGender(int drugId, int age, string gender)
        {
            // Find all of the drugs entries that match the age range and gender.
            QueryDefinition query = new QueryDefinition(@"
                SELECT
                    c.Id,
                    c.RegimenName,
                    c.CountryTreated,
                    c.RegimenId,
                    c.OutcomeComputed,
                    c.Unusual,
                    c.AdditionalInfo,
                    c.AdverseEvents
                FROM CaseFiles c
                JOIN r IN c.RegimenDrugs
                WHERE @age >= c.AgeLowerBound
                    AND @age <= c.AgeUpperBound
                    AND LOWER(c.Gender) = @gender
                    AND r.CureId = @drugId")
                .WithParameter("@age", age)
                .WithParameter("@gender", gender.ToLowerInvariant())
                .WithParameter("@drugId", drugId);

            List<RegimenResult> results = await Query<RegimenResult, DrugEntry>(query);

            return results;
        }
    }
}