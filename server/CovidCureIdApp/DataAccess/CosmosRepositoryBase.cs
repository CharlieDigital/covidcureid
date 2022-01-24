namespace CovidCureIdApp.DataAccess;

/// <summary>
///     Abstract base class for the Cosmos DB repositories.
/// </summary>
/// <remarks>
///     See notes here: https://joonasw.net/view/exploring-cosmos-db-sdk-v3
/// </remarks>
public abstract class CosmosRepositoryBase<T> where T : DomainEntityBase
{
    private readonly CosmosGateway _cosmos;

    /// <summary>
    ///     Injection constructor.
    /// </summary>
    /// <param name="cosmos">The injected Cosmos gateway.</param>
    protected CosmosRepositoryBase(CosmosGateway cosmos)
    {
        _cosmos = cosmos;
    }

    /// <summary>
    ///     Adds an instance of the entity to the Cosmos database.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    public virtual async Task<T> AddOrUpdate(T entity)
    {
        if (entity.Id == Guid.Empty.ToString())
        {
            // Apply a GUID if none is specified.
            entity.Id = Guid.NewGuid().ToString();
        }

        return await _cosmos.ExecuteItem(c => c.UpsertItemAsync(entity, new PartitionKey(entity.PartitionKey)));
    }

    /// <summary>
    ///     Updates an instance of the entity in the Cosmos database.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    public virtual async Task Update(T entity)
    {
        await _cosmos.ExecuteItem(c => c.ReplaceItemAsync(entity, entity.Id.ToString(), new PartitionKey(entity.PartitionKey)));
    }

    /// <summary>
    ///     Updates or inserts an entity.
    /// </summary>
    /// <param name="entity">The entity to update or insert.</param>
    /// <returns>The entity that was updated or inserted</returns>
    public virtual async Task<T> UpsertAsync(T entity)
    {
        return await _cosmos.ExecuteItem(c => c.UpsertItemAsync(entity, new PartitionKey(entity.PartitionKey)));
    }

    /// <summary>
    ///     Deletes an instance of the entity in the Cosmos database.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    public virtual async Task Delete(T entity)
    {
        await DeleteById(entity.ContainerName, entity.PartitionKey, entity.Id);
    }

    /// <summary>
    ///     Deletes an instance of the entity in the Cosmos database by UID.
    /// </summary>
    /// <param name="containerName">The container which the entity is located in.</param>
    /// <param name="id">The string ID of the entity to delete.</param>
    /// <param name="partitionKey">The partition key for the entity.</param>
    public virtual async Task DeleteById(string containerName, string partitionKey, string id)
    {
        await _cosmos.ExecuteItem(containerName, c => c.DeleteItemAsync<T>(id, new PartitionKey(partitionKey)));
    }

    /// <summary>
    ///     Returns a specific instance by UID.
    /// </summary>
    /// <param name="partitionKey">The partition key that the entity is associated with.</param>
    /// <param name="id">The string ID of the instance to return.</param>
    /// <returns>The instance of the document entity to return.</returns>
    public virtual async Task<T> GetById(string partitionKey, string id)
    {
        return await _cosmos.ExecuteItem(c => c.ReadItemAsync<T>(id, new PartitionKey(partitionKey)));
    }

    /// <summary>
    ///     Returns a specific instance by UID.
    /// </summary>
    /// <param name="id">The string ID of the instance to return.</param>
    /// <returns>The instance of the document entity to return.</returns>
    public virtual async Task<T> GetByIdAsync(string id)
    {
        return await Find(item => item.Id == id);
    }

    /// <summary>
    ///     Retrieve multiple entities by their IDs.
    /// </summary>
    /// <param name="entityIds">The IDs of the instances</param>
    /// <returns>An enumeration of the entities.</returns>
    public virtual async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> entityIds)
    {
        string sql = @"
            SELECT * FROM e
            WHERE e.TypeName = @TypeName AND ARRAY_CONTAINS(@EntityIds, e.id)";

        QueryDefinition query = new QueryDefinition(sql)
            .WithParameter("@TypeName", typeof(T).Name)
            .WithParameter("@EntityIds", entityIds);

        FeedResponse<T> response = await _cosmos.ExecuteIterator(c => c.GetItemQueryIterator<T>(query));

        return response;
    }

    /// <summary>
    ///     Finds an instance based on a Boolean property filter criteria.
    /// </summary>
    /// <param name="predicate">The property filter criteria which takes an instance of the type and returns a Boolean match on a property value.</param>
    /// <returns>An instance of the type.</returns>
    public virtual async Task<T> Find(Expression<Func<T, bool>> predicate)
    {
        // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container.getitemlinqqueryable?view=azure-dotnet

        // TODO: One of the other methods may be more efficient?

        FeedResponse<T> response = await _cosmos.ExecuteIterator(c => c
            .GetItemLinqQueryable<T>(true)
            .Where(predicate).ToFeedIterator());

        return response?.FirstOrDefault();
    }

    /// <summary>
    ///     Gets items of the specified type in paged sets which is ordered by the name in ascending order.
    /// </summary>
    /// <param name="startIndex">The zero based starting index.</param>
    /// <param name="pageSize">The size of each page of data.</param>
    /// <returns>A result set which includes a continuation token.</returns>
    public virtual async Task<List<T>> GetItems(int startIndex, int pageSize)
    {
        FeedResponse<T> response = await _cosmos.ExecuteIterator(c => c
            .GetItemLinqQueryable<T>(true)
            .Where(e => e.TypeName == typeof(T).Name)
            .OrderBy(e => e.Name)
            .Skip(startIndex)
            .Take(pageSize + 1)
            .ToFeedIterator());

        List<T> results = response == null ? new List<T>() : response.ToList();

        return results;
    }

    /// <summary>
    ///     Gets items of the specified type in paged sets which is ordered by the name in ascending order.
    /// </summary>
    /// <param name="startIndex">The zero based starting index.</param>
    /// <param name="pageSize">The size of each page of data.</param>
    /// <param name="filterPredicates">A set of filters to apply in the where clause of the query.</param>
    /// <param name="orderPredicate">The order predicate to apply.</param>
    /// <param name="sortDirection">The sorting direction for the query.</param>
    /// <returns>A result set which includes a continuation token.</returns>
    public virtual async Task<List<T>> GetItemsFiltered<TOrder>(int startIndex, int pageSize,
        Expression<Func<T, TOrder>> orderPredicate, SortDirection sortDirection, params Expression<Func<T, bool>>[] filterPredicates)
    {
        FeedResponse<T> response = await _cosmos.ExecuteIterator(c =>
        {
            IQueryable<T> query = c
                .GetItemLinqQueryable<T>(true)
                .Where(e => e.TypeName == typeof(T).Name);

            foreach (Expression<Func<T, bool>> filter in filterPredicates)
            {
                query = query.Where(filter);
            }

            query = sortDirection == SortDirection.Ascending
                ? query.OrderBy(orderPredicate)
                : query.OrderByDescending(orderPredicate);

            return query
                .Skip(startIndex)
                .Take(pageSize + 1)
                .ToFeedIterator();
        });

        List<T> results = response == null ? new List<T>() : response.ToList();

        return results;
    }

    /// <summary>
    ///     Convenience method which returns a single item.
    /// </summary>
    /// <param name="filterPredicates">A set of filters to apply in the where clause of the query.</param>
    /// <returns>A single item from the result set.</returns>
    public virtual async Task<T> GetItemFiltered(params Expression<Func<T, bool>>[] filterPredicates)
    {
        List<T> results = await GetItemsFiltered(0, 1, e => e.Name, SortDirection.Ascending, filterPredicates);

        if (results == null || results.Count == 0)
        {
            return null;
        }

        return results[0];
    }

    /// <summary>
    ///     Finds an instance based on a query and parameters.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="parameters">The list of parameters.</param>
    /// <param name="partitionKey">The partition key.</param>
    /// <returns>An instance of the type.</returns>
    public virtual async Task<T> Find(string query, Dictionary<string, object> parameters = null, string partitionKey = null)
    {
        // TODO: One of the other methods may be more efficient?
        QueryDefinition queryDefinition = new QueryDefinition(query);
        if (parameters != null)
        {
            foreach (var item in parameters)
            {
                queryDefinition.WithParameter(item.Key, item.Value);
            }
        }

        PartitionKey key = !string.IsNullOrWhiteSpace(partitionKey) ? new PartitionKey(partitionKey) : new PartitionKey(typeof(T).Name.ToLowerInvariant());

        FeedResponse<T> response = await _cosmos.ExecuteIterator(c => c
            .GetItemQueryIterator<T>(queryDefinition, requestOptions: new QueryRequestOptions() { PartitionKey = key }));

        return response?.FirstOrDefault();
    }

    /// <summary>
    ///     Executes a direct query against the Cosmos database typically representing an aggregate query.
    /// </summary>
    /// <param name="query">The query definition to execute.</param>
    /// <typeparam name="Tresponse">The type of the response entity which should represent an aggregate</typeparam>
    /// <typeparam name="Tentity">The type of the entity that is being queried</typeparam>
    /// <returns>The aggregate responses.</returns>
    public virtual async Task<List<Tresponse>> Query<Tresponse, Tentity>(QueryDefinition query) where Tentity : DomainEntityBase
    {
        return await _cosmos.ExecuteQuery<Tresponse, Tentity>(query);
    }
}
