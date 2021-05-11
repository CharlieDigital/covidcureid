using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CovidCureIdApp.DataAccess;

[assembly: FunctionsStartup(typeof(CovidCureIdApp.AppStartup))]

namespace CovidCureIdApp
{
    /// <summary>
    ///     Startup class used to initialize the dependency injection.
    /// </summary>
    /// <remarks>
    ///     See: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
    /// </remarks>
    public class AppStartup : FunctionsStartup
    {
        /// <summary>
        ///     Initializes the dependency injection container.
        /// </summary>
        /// <param name="builder">The handle to the host builder.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Registrations for domain data access.
            builder.Services.AddSingleton(new CosmosClient(
                    Environment.GetEnvironmentVariable("CosmosEndpoint"),
                    Environment.GetEnvironmentVariable("CosmosAuthKey")));

            builder.Services.AddSingleton<CosmosGateway>();
            builder.Services.AddSingleton<RegimenRepository>();
            builder.Services.AddSingleton<DrugRepository>();
        }
    }
}