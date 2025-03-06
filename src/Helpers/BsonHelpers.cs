namespace Gabonet.MongoRepository.Helpers;

using MongoDB.Bson;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;

public static class BsonHelpers
{
    public static string? GetBsonElementName<T>(string propertyName)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (propertyInfo != null)
        {
            var bsonElementAttribute = propertyInfo.GetCustomAttribute<BsonElementAttribute>();
            return bsonElementAttribute?.ElementName;
        }

        return null;
    }

    public static ObjectId ToObjectId(object id)
    {
        return id switch
        {
            string idString => new ObjectId(idString),
            ObjectId objectId => objectId,
            _ => throw new ArgumentException("El ID proporcionado no es válido para ObjectId.", nameof(id))
        };
    }
}