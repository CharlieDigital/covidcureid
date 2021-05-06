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
    }
}