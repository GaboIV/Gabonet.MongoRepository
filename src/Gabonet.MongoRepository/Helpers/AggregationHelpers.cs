namespace Gabonet.MongoRepository.Helpers;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

public static class AggregationHelpers
{
    public static IAggregateFluent<BsonDocument> ApplyMatch<T>(
    IAggregateFluent<BsonDocument> aggregation,
    FilterDefinition<T> filterDefinition)
    {
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<T>();

        var bsonFilter = filterDefinition.Render(documentSerializer, serializerRegistry);

        if (!bsonFilter.Elements.Any())
        {
            return aggregation;
        }

        return aggregation.Match(bsonFilter);
    }

    public static IAggregateFluent<BsonDocument> ApplyAggregationMatch<T>(
        IAggregateFluent<BsonDocument> aggregation,
        string aggregationFilter)
    {
        var filter = BsonDocument.Parse(aggregationFilter);
        return aggregation.Match(filter);
    }
}