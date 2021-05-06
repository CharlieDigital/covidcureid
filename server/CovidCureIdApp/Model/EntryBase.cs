namespace CovidCureIdApp.Model
{
    /// <summary>
    ///     Base class for an entry whether it's a single drug or a regimen.
    /// </summary>
    public abstract class EntryBase
    {
        /// <summary>
        ///     The ID for this entry.
        /// </summary>
        public string Id;

        /// <summary>
        ///     The root CURe ID that the entry is generated from.
        /// </summary>
        public int CureId;

        /// <summary>
        ///     The lower bound age bracket for the individual in the case
        /// </summary>
        public int AgeLowerBound;

        /// <summary>
        ///     The upper bound age bracket for the individual in the case
        /// </summary>
        public int AgeUpperBound;

        /// <summary>
        ///     The gender of the individual in the case
        /// </summary>
        public string Gender;

        /// <summary>
        ///     Country where the individual was treated
        /// </summary>
        public string CountryTreated;

        /// <summary>
        ///     A string array of races identified for the individual
        /// </summary>
        public string[] Races;

        /// <summary>
        ///     The discrete outcome for the individual.  For example: "Patient died"
        /// </summary>
        public string Outcome;

        /// <summary>
        ///     The computed outcome for the individual.  For example: "Deteriorated"
        /// </summary>
        public string OutcomeComputed;

        /// <summary>
        ///     The year of the treatment regimen
        /// </summary>
        public int TreatmentYear;

        /// <summary>
        ///     The type of the entry for query filtering purposes.
        /// </summary>
        /// <value>A string value indicating whether type is a drug or a regimen.</value>
        public abstract string EntryType {
            get;
            set;
        }
    }
}