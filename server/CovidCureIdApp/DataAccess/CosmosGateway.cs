using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using CovidCureIdApp.Model;
using CovidCureIdApp.DataAccess.Support;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace CovidCureIdApp.DataAccess
{
    /// <summary>
    ///     Cosmos wrapper class for database management.
    /// </summary>
    /// <remarks>
    ///     https://github.com/Azure/azure-cosmos-dotnet-v3/tree/master/Microsoft.Azure.Cosmos.Samples/CodeSamples
    /// </remarks>
    public class CosmosGateway
    {
        public static readonly string DatabaseName = "CovidCureId";

        private CosmosClient _client;
        private readonly ILogger _log;

        private static readonly IDictionary<Type, Container> ContainersByType = new ConcurrentDictionary<Type, Container>();

        /// <summary>
        ///     Injection constructor.
        /// </summary>
        /// <param name="log">The injected logger.</param>
        /// <param name="client">The injected Cosmos client.</param>
        public CosmosGateway(ILogger<CosmosGateway> log, CosmosClient client)
        {
            _log = log;
            _client = client;
        }

        /// <summary>
        ///     Gets a container from the default database using the specified name.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>An instance of the specified container.</returns>
        public Container GetContainer(string containerName)
        {
            return _client.GetContainer(DatabaseName, containerName);
        }

        /// <summary>
        ///     Gets a container from the default database using the specified name.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <returns>An instance of the specified container.</returns>
        public Container GetContainer<T>() where T : DomainEntityBase
        {
            Type type = typeof(T);

            if (ContainersByType.ContainsKey(type))
            {
                return ContainersByType[type];
            }

            ContainerAttribute containerDefinition = type.GetCustomAttribute<ContainerAttribute>(false);

            string containerName = (string.IsNullOrEmpty(containerDefinition.Name) ? type.Name : containerDefinition.Name);

            Container container = GetContainer(containerName);

            // cache it
            ContainersByType[type] = container;

            return container;
        }

        /// <summary>
        ///     Executes an item action against the container with timings.
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="action">The action to execute against the container.</param>
        /// <returns>The response from the server.</returns>
        public async Task<ItemResponse<T>> ExecuteItem<T>(Func<Container, Task<ItemResponse<T>>> action) where T : DomainEntityBase
        {
            return await ExecuteItem(GetContainer<T>(), action);
        }

        /// <summary>
        ///     Executes an item action against the container with timings.
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="action">The action to execute against the container.</param>
        /// <returns>The response from the server.</returns>
        public async Task<ItemResponse<T>> ExecuteItem<T>(string containerName, Func<Container, Task<ItemResponse<T>>> action) where T : DomainEntityBase
        {
            return await ExecuteItem(GetContainer(containerName), action);
        }

        /// <summary>
        ///     Executes an item action against the container with timings.
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="container">The container instance.</param>
        /// <param name="action">The action to execute against the container.</param>
        /// <returns>The response from the server.</returns>
        public async Task<ItemResponse<T>> ExecuteItem<T>(Container container, Func<Container, Task<ItemResponse<T>>> action) where T : DomainEntityBase
        {
            Stopwatch timer = new Stopwatch();

            timer.Start();

            ItemResponse<T> response = await action(container);

            timer.Stop();

            _log.LogWarning($"Action returned status [{response.StatusCode.ToString()}] in [{timer.ElapsedMilliseconds}] ms and consumed [{response.RequestCharge}] RU(s).");

            return response;
        }

        /// <summary>
        ///     Executes an iterator action against the container with timings.
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="action">The action to execute against the container.</param>
        /// <returns>The response from the server.</returns>
        public async Task<FeedResponse<T>> ExecuteIterator<T>(Func<Container, FeedIterator<T>> action) where T : DomainEntityBase
        {
            return await ExecuteIterator(GetContainer<T>(), action);
        }

        /// <summary>
        ///     Executes an iterator action against the container with timings.
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="container">The container instance.</param>
        /// <param name="action">The action to execute against the container.</param>
        /// <returns>The response from the server.</returns>
        public async Task<FeedResponse<T>> ExecuteIterator<T>(Container container, Func<Container, FeedIterator<T>> action) where T : DomainEntityBase
        {
            Stopwatch timer = new Stopwatch();

            timer.Start();

            FeedIterator<T> iterator = action(container);

            FeedResponse<T> response = null;

            if (iterator.HasMoreResults)
            {
                response = await iterator.ReadNextAsync();

                timer.Stop();

                _log.LogWarning($"Action returned status [{response.StatusCode.ToString()}] in [{timer.ElapsedMilliseconds}] ms and consumed [{response.RequestCharge}] RU(s).");
            }

            return response;
        }

        /// <summary>
        ///     Resets the database by deleting the database and then initializing it again.
        /// </summary>
        public async Task ResetDatabase()
        {
            DatabaseResponse response = await _client.GetDatabase(DatabaseName).DeleteAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                await InitializeDatabase();
            }
            else
            {
                throw new InvalidOperationException($"The delete operation failed with status: {response.StatusCode.ToString()}");
            }
        }

        /// <summary>
        ///     Initializes the database, collections, indexes, and partition keys.
        /// </summary>
        public async Task InitializeDatabase()
        {
            DatabaseResponse response = await _client.CreateDatabaseIfNotExistsAsync(DatabaseName);

            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            Dictionary<string, ContainerProperties> containers = new Dictionary<string, ContainerProperties>();

            foreach (Type type in types)
            {
                ContainerAttribute containerSpecification = type.GetCustomAttribute<ContainerAttribute>(false);

                if (containerSpecification == null)
                {
                    continue;
                }

                string containerName = type.Name;

                if (containerSpecification != null && !string.IsNullOrEmpty(containerSpecification.Name))
                {
                    containerName = containerSpecification.Name;
                }

                if (!containers.ContainsKey(containerName))
                {
                    // Add to the dictionary if we don't have one already for this name.
                    containers[containerName] = new ContainerProperties { Id = containerName };
                }

                ContainerProperties containerProperties = containers[containerName];
            }

            // Iterate the containers and provision them.
            foreach (string key in containers.Keys)
            {
                // All containers have explicit ParititionKey definition.
                containers[key].PartitionKeyPath = "/PartitionKey";

                ContainerResponse containerResponse = await response.Database.CreateContainerIfNotExistsAsync(containers[key]);
            }
        }
    }
}
