namespace Gabonet.MongoRepository.Helpers;

using GabonetCard.Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

public static class SortingHelpers
{
    public static IAggregateFluent<BsonDocument> BuildSorting<T>(IAggregateFluent<BsonDocument> aggregation, string? orderColumn, string? orderBy)
    {
        if (string.IsNullOrEmpty(orderColumn))
        {
            return aggregation;
        }
        
        var lookupDictionary = LookupDictionaryBase<T>.GetLookupDictionary();
        
        if (lookupDictionary!=null && lookupDictionary.SortDictionary.TryGetValue(orderColumn.ToLower(), out string? mappedValue))
        {
            orderColumn = mappedValue;
        }

        int sortOrder = orderBy?.ToLower() == "desc" ? -1 : 1;

        var sortStage = new BsonDocument("$sort", new BsonDocument(orderColumn, sortOrder));
        aggregation = aggregation.AppendStage<BsonDocument>(sortStage);

        return aggregation;
    }
}