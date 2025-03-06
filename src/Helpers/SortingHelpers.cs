namespace Gabonet.MongoRepository.Helpers;

using GabonetCard.Infrastructure.Helpers;
using MongoDB.Bson;
using MongoDB.Driver;

public static class SortingHelpers
{
    public static IAggregateFluent<BsonDocument> BuildSorting<T>(IAggregateFluent<BsonDocument> aggregation, string? orderColumn, string? orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderColumn))
        {
            return aggregation;
        }

        string mappedColumn = orderColumn.ToLower() switch
        {
            "nombre" => "name",
            "fecha" => "date",
            _ => orderColumn
        };

        string[] parts = mappedColumn.Split('.');
        string bsonOrderColumn = BsonHelpers.GetBsonElementName<T>(parts[0]) ?? parts[0];

        if (string.IsNullOrWhiteSpace(bsonOrderColumn))
        {
            return aggregation;
        }

        int sortOrder = string.Equals(orderBy?.ToLower(), "desc", StringComparison.OrdinalIgnoreCase) ? -1 : 1;

        var sortStage = new BsonDocument("$sort", new BsonDocument(bsonOrderColumn, sortOrder));
        aggregation = aggregation.AppendStage<BsonDocument>(sortStage);

        return aggregation;
    }
}