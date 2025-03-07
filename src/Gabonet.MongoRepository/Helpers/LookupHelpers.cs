namespace Gabonet.MongoRepository.Helpers;

using Gabonet.MongoRepository.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

public static class LookupHelpers
{
    public static IAggregateFluent<BsonDocument> ApplyLookups<T>(IAggregateFluent<BsonDocument> aggregation, List<string>? lookups) where T: class
    {
        var properties = typeof(T).GetProperties()
          .Where(p => Attribute.IsDefined(p, typeof(MongoLookupAttribute)))
          .ToList();

        foreach (var property in properties)
        {
            var lookupAttr = Attribute.GetCustomAttribute(property, typeof(MongoLookupAttribute)) as MongoLookupAttribute;

            if (lookupAttr != null && (lookups == null || lookups.Contains(property.Name)))
            {
                string alias = property.Name;

                aggregation = aggregation.Lookup<BsonDocument, BsonDocument>(
                    lookupAttr.CollectionName,
                    lookupAttr.LocalField,
                    lookupAttr.ForeignField,
                    alias
                );

                bool isList = property.PropertyType.IsGenericType &&
                              property.PropertyType.GetGenericTypeDefinition() == typeof(List<>);

                if (!isList)
                {
                    aggregation = aggregation.AppendStage<BsonDocument>(
                        $"{{ $addFields: {{ {alias}: {{ $arrayElemAt: [\"${alias}\", 0] }} }} }}"
                    );
                }

                var nestedType = property.PropertyType;
                if (!isList && nestedType.IsClass && nestedType!= typeof(string))
                {
                    var nestedProperties = nestedType.GetProperties()
                      .Where(p => Attribute.IsDefined(p, typeof(MongoLookupAttribute)))
                      .ToList();

                    if (nestedProperties.Any())
                    {
                        aggregation = ApplyNestedLookups<T>(aggregation, nestedType, alias);
                    }
                }
            }
        }

        return aggregation;
    }

    public static IAggregateFluent<BsonDocument> ApplyNestedLookups<T>(IAggregateFluent<BsonDocument> aggregation, Type nestedType, string parentAlias) where T: class
    {
        var nestedProperties = nestedType.GetProperties()
          .Where(p => Attribute.IsDefined(p, typeof(MongoLookupAttribute)))
          .ToList();

        foreach (var nestedProperty in nestedProperties)
        {
            var nestedLookupAttr = Attribute.GetCustomAttribute(nestedProperty, typeof(MongoLookupAttribute)) as MongoLookupAttribute;

            if (nestedLookupAttr!= null)
            {
                string nestedAlias = $"{parentAlias}.{nestedProperty.Name}";

                aggregation = aggregation.Lookup<BsonDocument, BsonDocument>(
                    nestedLookupAttr.CollectionName,
                    $"{parentAlias}.{nestedLookupAttr.LocalField}",
                    nestedLookupAttr.ForeignField,
                    nestedAlias
                );

                bool isList = nestedProperty.PropertyType.IsGenericType &&
                              nestedProperty.PropertyType.GetGenericTypeDefinition() == typeof(List<>);

                if (!isList)
                {
                    aggregation = aggregation.AppendStage<BsonDocument>(
                        $"{{ $addFields: {{ \"{nestedAlias}\": {{ $arrayElemAt: [\"${nestedAlias}\", 0] }} }} }}"
                    );
                }

                var nextNestedType = nestedProperty.PropertyType;
                if (!isList && nextNestedType.IsClass && nextNestedType!= typeof(string))
                {
                    aggregation = ApplyNestedLookups<T>(aggregation, nextNestedType, nestedAlias);
                }
            }
        }

        return aggregation;
    }
}