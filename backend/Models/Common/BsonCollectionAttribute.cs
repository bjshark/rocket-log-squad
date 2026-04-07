namespace RocketLog.Api.Models.Common;

[AttributeUsage(AttributeTargets.Class)]
public sealed class BsonCollectionAttribute : Attribute
{
    public BsonCollectionAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }

    public string CollectionName { get; }
}