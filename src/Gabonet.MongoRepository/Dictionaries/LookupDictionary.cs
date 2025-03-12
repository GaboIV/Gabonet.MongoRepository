using System.Reflection;

namespace GabonetCard.Infrastructure.Helpers;

public enum PropertyType
{
    ObjectId,
    String,
    Number,
    DateTime,
    Boolean,
    Guid,
    Decimal
}

public abstract class LookupDictionaryBase<T>
{
    public abstract Dictionary<string, string> SortDictionary { get; }
    public abstract Dictionary<string, (string property, string filterType, PropertyType propertyType)> FilterDictionary { get; }

    private static readonly Dictionary<Type, LookupDictionaryBase<T>> _dictionaryCache = new();

    public static LookupDictionaryBase<T>? GetLookupDictionary()
    {
        var type = typeof(T);

        if (_dictionaryCache.TryGetValue(type, out var cachedDictionary))
        {
            return cachedDictionary;
        }

        var dictionaryType = typeof(LookupDictionaryBase<>);
        var assembly = Assembly.GetExecutingAssembly();

        var dictionaryImplementations = assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType &&
                        t.BaseType.GetGenericTypeDefinition() == dictionaryType)
            .ToList();

        foreach (var implementation in dictionaryImplementations)
        {
            var genericArgument = implementation.BaseType?.GetGenericArguments().FirstOrDefault();
            if (genericArgument == type)
            {
                var instance = Activator.CreateInstance(implementation) as LookupDictionaryBase<T>;
                if (instance != null)
                {
                    _dictionaryCache[type] = instance;
                    return instance;
                }
            }
        }

        return null;
    }
}