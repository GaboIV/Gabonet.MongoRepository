namespace Gabonet.MongoRepository.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MongoLookupAttribute : Attribute
{
    public string CollectionName { get; }
    public string LocalField { get; }
    public string ForeignField { get; }
    public string OrderProperty { get; }
    public string Limit { get; }

    public MongoLookupAttribute(string collectionName, string localField, string foreignField, string orderProperty = "", string limit = "")
    {
        CollectionName = collectionName;
        LocalField = localField;
        ForeignField = foreignField;
        OrderProperty = OrderProperty ?? "";
        Limit = Limit ?? "";
    }
}