using System;
using System.Text.Json;

namespace CovidCureIdApp.Model
{
    /// <summary>
    ///     Represents a single drug entry in the CURE ID database.  This data type breaks apart the
    ///     regimens into individual drugs.
    /// </summary>
    public class DrugEntry : EntryBase
    {
        /// <summary>
        ///     The numeric ID of the drug
        /// </summary>
        public int DrugId;

        /// <summary>
        ///     The string display name of the drug
        /// </summary>
        public string DrugName;

        /// <summary>
        ///     The type of the entry for query filtering purposes.
        /// </summary>
        /// <value>A string value indicating whether type is a drug or a regimen.</value>
        public override string EntryType {
            get => "Drug";
            set { /* Needed for serialization */ }
        }

        /// <summary>
        ///     Static factory method to create an instance from a JSON element.
        /// </summary>
        /// <param name="drugId">The numeric ID of the drug from CURE ID.</param>
        /// <param name="drugName">The name of the drug from CURE ID.</param>
        /// <param name="json">The root JSON element from the CURE ID data files.</param>
        /// <returns>An instance constructed from the JSON element.</returns>
        public static DrugEntry From(int drugId, string drugName, JsonElement json) {
            string ageRange = json.GetProperty("age").GetString();
            string[] ageParts = ageRange.Split('-');
            int ageLower = Convert.ToInt32(ageParts[0]);
            int ageUpper = Convert.ToInt32(ageParts[1].Split(' ')[0]);

            DrugEntry entry = new DrugEntry{
                Id = Guid.NewGuid().ToString(),
                DrugId = drugId,
                DrugName = drugName,
                CureId = json.GetProperty("id").GetInt32(),
                AgeLowerBound = ageLower,
                AgeUpperBound = ageUpper,
                Gender = json.GetProperty("sex").GetString(),
                CountryTreated = json.GetProperty("country_treated").GetString(),
                Races = new string[] {""},
                Outcome = json.GetProperty("outcome").GetString(),
                OutcomeComputed = json.GetProperty("outcome_computed").GetString(),
                TreatmentYear = Convert.ToInt32(json.GetProperty("began_treatment_year").GetString())
            };

            return entry;
        }
    }
}