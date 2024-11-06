namespace Roller.Infrastructure.Domain;

public interface IEntityBase<T> : IPrimaryKey<T>, IDeletable, IDateAbility, IOperateAbility<T> where T : IEquatable<T>
{
    string? Name { get; set; }
}