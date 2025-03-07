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

public class LookupDictionary
{
    public static readonly Dictionary<string, string> sortDictionary = new()
    { };

    public static readonly Dictionary<string, (string property, string filterType, PropertyType propertyType)> filterDictionary = new()
    { };
}