namespace Gabonet.MongoRepository.Helpers;

using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;
using GabonetCard.Infrastructure.Helpers;
using Gabonet.MongoRepository.Attributes;

public static class FilterHelpers
{
    public static FilterDefinition<T> BuildFilters<T>(Dictionary<string, string>? filters)
    {
        var filterBuilder = Builders<T>.Filter;
        var filterDefinition = filterBuilder.Empty;

        if (filters == null || !filters.Any()) return filterDefinition;

        var filterList = new List<FilterDefinition<T>>();

        foreach (var filter in filters)
        {
            var isMapped = false;
            var propertyName = filter.Key;
            PropertyType propertyType = PropertyType.ObjectId;
            var filterType = GetFilterType<T>(propertyName);

            if (LookupDictionary.filterDictionary.TryGetValue(propertyName.ToLower(), out var mappedValue)) 
            {
                propertyName = mappedValue.property;
                filterType = mappedValue.filterType;
                propertyType = mappedValue.propertyType;
                isMapped = true;
            }

            var propertyValue = filter.Value;
            var bsonElementName = BsonHelpers.GetBsonElementName<T>(propertyName);

            if (isMapped || (bsonElementName != null && typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) != null))
            {
                FilterDefinition<T> filterExpression;
                if ("Precise".Equals(filterType, StringComparison.OrdinalIgnoreCase))
                {
                    if (ObjectId.TryParse(propertyValue, out var objectId) || (isMapped && propertyType.Equals(PropertyType.ObjectId)))
                    {
                        filterExpression = isMapped
                            ? filterBuilder.Eq(propertyName, ObjectId.Parse(propertyValue))
                            : filterBuilder.Eq(bsonElementName, objectId);
                    }
                    else if (DateTime.TryParse(propertyValue, out var dateValue) || (isMapped && propertyType.Equals(PropertyType.DateTime)))
                    {
                        filterExpression = isMapped
                            ? filterBuilder.Eq(propertyName, DateTime.Parse(propertyValue))
                            : filterBuilder.Eq(bsonElementName, dateValue);
                    }
                    else if (decimal.TryParse(propertyValue, out var numericValue) || (isMapped && propertyType.Equals(PropertyType.Decimal)))
                    {
                        filterExpression = isMapped
                            ? filterBuilder.Eq(propertyName, decimal.Parse(propertyValue))
                            : filterBuilder.Eq(bsonElementName, numericValue);
                    }
                    else
                    {
                        filterExpression = isMapped
                            ? filterBuilder.Regex(propertyName, new BsonRegularExpression($"^{propertyValue}$", "i"))
                            : filterBuilder.Regex(bsonElementName, new BsonRegularExpression($"^{propertyValue}$", "i"));
                    }
                }
                else
                {
                    var searchTerms = propertyValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (searchTerms.Length == 0) continue;

                    var regexFilters = searchTerms.Select(term =>
                        filterBuilder.Regex(bsonElementName, new BsonRegularExpression(term, "i")));

                    filterExpression = regexFilters.Count() == 1
                        ? regexFilters.First()
                        : filterBuilder.And(regexFilters);
                }

                filterList.Add(filterExpression);
            }
        }

        return filterList.Any() ? filterBuilder.And(filterList) : filterDefinition;
    }

    public static string? GetFilterType<T>(string propertyName)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo != null)
        {
            var filterTypeAttribute = propertyInfo.GetCustomAttribute<FilterTypeAttribute>();

            if (filterTypeAttribute != null)
            {
                return filterTypeAttribute.FilterType;
            }
        }

        return "Like";
    }
}