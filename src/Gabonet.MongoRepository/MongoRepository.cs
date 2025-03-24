namespace Gabonet.MongoRepository;

using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using Gabonet.MongoRepository.Helpers;
using Gabonet.MongoRepository.Abstractions;
using Gabonet.MongoRepository.Abstractions.DTOs;
using Gabonet.MongoRepository.Abstractions.Request;

public class MongoRepository<T> : IMongoRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public MongoRepository(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(object id, List<string>? lookups = null)
    {
        var objectId = BsonHelpers.ToObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);

        var aggregation = _collection.Aggregate().Match(filter).As<BsonDocument>();
        aggregation = LookupHelpers.ApplyLookups<T>(aggregation, lookups);

        var resultBson = await aggregation.FirstOrDefaultAsync();
        return resultBson == null ? null : BsonSerializer.Deserialize<T>(resultBson);
    }

    public async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<object> ids, List<string>? lookups = null)
    {
        var objectIds = ids.Select(BsonHelpers.ToObjectId);
        var filter = Builders<T>.Filter.In("_id", objectIds);

        var aggregation = _collection.Aggregate().Match(filter).As<BsonDocument>();
        aggregation = LookupHelpers.ApplyLookups<T>(aggregation, lookups ?? new List<string>());

        var resultBson = await aggregation.ToListAsync();
        return resultBson.Select(doc => BsonSerializer.Deserialize<T>(doc));
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity, object id)
    {
        var objectId = BsonHelpers.ToObjectId(id);
        await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", objectId), entity);
    }

    public async Task DeleteAsync(object id)
    {
        await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
    }
    
    public async Task<IEnumerable<T>> GetAllWithFiltersAsync(PagedRequest request, List<string>? lookups = null)
    {
        var filters = request.Filters ?? new Dictionary<string, string>();
        var filterDefinition = FilterHelpers.BuildFilters<T>(filters);

        var aggregation = _collection.Aggregate().As<BsonDocument>();
        aggregation = AggregationHelpers.ApplyMatch<T>(aggregation, filterDefinition);

        if (filters.TryGetValue("aggregation", out var aggregationFilter))
        {
            aggregation = AggregationHelpers.ApplyAggregationMatch<T>(aggregation, aggregationFilter);
        }

        aggregation = SortingHelpers.BuildSorting<T>(aggregation, request.OrderColumn, request.OrderBy);
        aggregation = LookupHelpers.ApplyLookups<T>(aggregation, lookups);

        var items = await aggregation.As<T>().ToListAsync();

        return items;
    }

    public async Task<PaginatedDataDto<T>> GetPagedAsync(PagedRequest request, List<string>? lookups = null)
    {
        var filters = request.Filters ?? new Dictionary<string, string>();
        var filterDefinition = FilterHelpers.BuildFilters<T>(filters);

        var totalRecords = await _collection.CountDocumentsAsync(filterDefinition);
        var totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

        var aggregation = _collection.Aggregate().As<BsonDocument>();
        
        aggregation = AggregationHelpers.ApplyMatch<T>(aggregation, filterDefinition);

        if (filters.TryGetValue("aggregation", out var aggregationFilter))
        {
            aggregation = AggregationHelpers.ApplyAggregationMatch<T>(aggregation, aggregationFilter);
        }

        aggregation = SortingHelpers.BuildSorting<T>(aggregation, request.OrderColumn, request.OrderBy);
        aggregation = LookupHelpers.ApplyLookups<T>(aggregation, lookups);

        var skip = (request.PageNumber - 1) * request.PageSize;
        aggregation = aggregation.Skip(skip).Limit(request.PageSize);

        var items = totalRecords > 0
            ? await aggregation.As<T>().ToListAsync()
            : new List<T>();

        return new PaginatedDataDto<T>
        {
            Items = items,
            Pagination = new PaginationInfo
            {
                TotalPages = totalPages,
                TotalItems = (int)totalRecords,
                CurrentPage = request.PageNumber,
                ItemsPerPage = request.PageSize
            }
        };
    }
}