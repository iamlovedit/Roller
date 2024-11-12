using MongoDB.Bson.Serialization.Attributes;

namespace Roller.Infrastructure.Repository.Mongo;

public interface IIdentifiable<TKey> where TKey : IEquatable<TKey>
{
    [BsonId] TKey Id { get; set; }
    string Name { get; set; }
}