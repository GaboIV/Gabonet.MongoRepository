namespace Gabonet.MongoRepository.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FilterTypeAttribute : Attribute
{
    public string FilterType { get; }

    public FilterTypeAttribute(string filterType)
    {
        FilterType = filterType;
    }
}