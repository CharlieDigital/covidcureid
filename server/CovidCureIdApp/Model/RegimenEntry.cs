using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CovidCureIdApp.DataAccess.Support;

namespace CovidCureIdApp.Model
{
    /// <summary>
    ///     Represents the regimen identified by a single case file in the CURE ID database
    /// </summary>
    [Container("CaseFiles")]
    public class RegimenEntry : EntryBase
    {
        /// <summary>
        ///     The regimen ID which is the same as the case ID in CURE.
        /// </summary>
        public int RegimenId;

        /// <summary>
        ///     The name of the regimen as a combination of the individual drug names
        /// </summary>
        public string RegimenName;

        /// <summary>
        ///     The
        /// </summary>
        public Drug[] RegimenDrugs;

        /// <summary>
        ///     The type of the entry for query filtering purposes.
        /// </summary>
        /// <value>A string value indicating whether type is a drug or a regimen.</value>
        public override string EntryType {
            get => "Regimen";
            set { /* Needed for serialization */ }
        }

        public override string PartitionKey => Guid.NewGuid().ToString();

        /// <summary>
        ///     Static factory method to create an instance from a JSON element.
        /// </summary>
        /// <param name="json">The root JSON element from the CURE ID data files.</param>
        /// <returns>An instance constructed from the JSON element.</returns>
        internal static RegimenEntry From(JsonElement json)
        {
            string ageRange = json.GetProperty("age").GetString();

            Age age = new Age(ageRange);

            // Construct the regimen ID based on the ordered IDs of the drugs in the regimen.
            IEnumerable<Drug> drugs = json.GetProperty("regimens").EnumerateArray().Select(element => {
                return new Drug {
                    CureId = element.GetProperty("drug").GetProperty("id").GetInt32(),
                    Name = element.GetProperty("drug").GetProperty("name").GetString()
                };
            }).OrderBy(drug => drug.CureId);

            int treatmentYear = json.GetProperty("began_treatment_year").GetString() == string.Empty?
                json.GetProperty("pub_year").GetInt32() :
                Convert.ToInt32(json.GetProperty("began_treatment_year").GetString());

            string computedOutcome = json.GetProperty("outcome_computed").GetString();

            RegimenEntry entry = new RegimenEntry{
                Id = Guid.NewGuid().ToString(),
                RegimenId = json.GetProperty("id").GetInt32(),
                RegimenName = String.Join("+", drugs.Select(d => d.Name)),
                RegimenDrugs = drugs.ToArray(),
                CureId = json.GetProperty("id").GetInt32(),
                AgeLowerBound = age.Lower,
                AgeUpperBound = age.Upper,
                Gender = json.GetProperty("sex").GetString(),
                CountryTreated = json.GetProperty("country_treated").GetString(),
                Races = new string[] {""},
                Outcome = json.GetProperty("outcome").GetString(),
                OutcomeComputed = computedOutcome,
                Improved = computedOutcome.Equals("improved", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0,
                Deteriorated = computedOutcome.Equals("deteriorated", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0,
                Undetermined = computedOutcome.Equals("undetermined", StringComparison.InvariantCultureIgnoreCase) ? 1 : 0,
                TreatmentYear = treatmentYear
            };

            return entry;
        }
    }
}