using CovidCureIdApp.DataAccess;

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
    }
}