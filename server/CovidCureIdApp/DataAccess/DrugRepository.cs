using CovidCureIdApp.Model;

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
    }
}