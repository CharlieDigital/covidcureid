namespace CovidCureIdApp.Model
{
    /// <summary>
    ///     Represents a regimen result returned from querying Cosmos.
    /// </summary>
    public class RegimenResult
    {
        /// <summary>
        ///     The unique ID of this entry.
        /// </summary>
        public string Id;

        /// <summary>
        ///     The ID of the regimen and should match to the original ID of the case in CURE ID
        /// </summary>
        public int RegimenId;

        /// <summary>
        ///     The name of the regimen which includes all drug names
        /// </summary>
        public string RegimenName;

        /// <summary>
        ///     The country that the case was treated in.
        /// </summary>
        public string CountryTreated;

        /// <summary>
        ///     The computed outcome display string.
        /// </summary>
        public string OutcomeComputed;

        /// <summary>
        ///     Narrative notes on any unusual outcomes for this case.
        /// </summary>
        public string Unusual;

        /// <summary>
        ///     Narrative notes on adverse events associated with this case.
        /// </summary>
        public string AdverseEvents;

        /// <summary>
        ///     Narrative notes on additional info associated with this case.
        /// </summary>
        public string AdditionalInfo;
    }
}