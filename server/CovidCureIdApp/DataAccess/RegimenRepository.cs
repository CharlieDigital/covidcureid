using CovidCureIdApp.Model;

namespace CovidCureIdApp.DataAccess
{
    /// <summary>
    ///     Repository class for interfacing with the Regimens stored in the system.
    /// </summary>
    public class RegimenRepository : CosmosRepositoryBase<RegimenEntry>
    {
        /// <summary>
        ///     Injection constructor.
        /// </summary>
        /// <param name="cosmos">The instance of the gateway used for connecting to Cosmos</param>
        /// <returns>An instance of the repository.</returns>
        public RegimenRepository(CosmosGateway cosmos) : base(cosmos)
        {
        }
    }
}