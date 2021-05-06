using System;
using System.Text.Json;

namespace CovidCureIdApp.Model
{
    /// <summary>
    ///     Represents the regimen identified by a single case file in the CURE ID database
    /// </summary>
    public class RegimenEntry : EntryBase
    {
        /// <summary>
        ///     The regimen ID as a combination of the individual drugs
        /// </summary>
        public string RegimenId;

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

        /// <summary>
        ///     Static factory method to create an instance from a JSON element.
        /// </summary>
        /// <param name="json">The root JSON element from the CURE ID data files.</param>
        /// <returns>An instance constructed from the JSON element.</returns>
        internal static RegimenEntry From(JsonElement json)
        {
            return null;
        }
    }
}