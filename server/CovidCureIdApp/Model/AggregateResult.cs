namespace CovidCureIdApp.Model
{
    /// <summary>
    ///     Represents an aggregate result returned from querying Cosmos.
    /// </summary>
    public class AggregateResult
    {
        /// <summary>
        ///     The name of the drug this aggregate result is for.
        /// </summary>
        public string DrugName;

        /// <summary>
        ///     The ID of the drug as it exists in CURE.
        /// </summary>
        public int DrugId;

        /// <summary>
        ///     The aggregate number of cases that improved.
        /// </summary>
        public int Improved;

        /// <summary>
        ///     The aggregate number of cases that deteriorated.
        /// </summary>
        public int Deteriorated;

        /// <summary>
        ///     The aggregate number of cases that are undetermined.
        /// </summary>
        public int Undetermined;
    }
}