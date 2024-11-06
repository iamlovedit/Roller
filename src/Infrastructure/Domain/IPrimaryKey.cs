namespace Roller.Infrastructure.Domain;

public interface IPrimaryKey<T> where T : IEquatable<T>
{
    T Id { get; set; }
}