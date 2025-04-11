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

        var renderArgs = new RenderArgs<T>
        {
            DocumentSerializer = documentSerializer,
            SerializerRegistry = serializerRegistry
        };

        var bsonFilter = filterDefinition.Render(renderArgs);

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

    public static IAggregateFluent<BsonDocument> ApplySortPipeline<T>(
        IAggregateFluent<BsonDocument> aggregation,
        string sortPipeline)
    {
        try 
        {
            // Si el pipeline no está en formato de array, lo convertimos
            if (!sortPipeline.Trim().StartsWith("["))
            {
                sortPipeline = "[" + sortPipeline + "]";
            }
            
            var pipelineArray = BsonSerializer.Deserialize<BsonArray>(sortPipeline);

            foreach (var stage in pipelineArray)
            {
                aggregation = aggregation.AppendStage<BsonDocument>(stage.AsBsonDocument);
            }

            return aggregation;
        }
        catch
        {
            // Log error if needed
            // En caso de error, devolvemos la agregación sin cambios
            return aggregation;
        }
    }
}