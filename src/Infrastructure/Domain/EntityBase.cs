using SqlSugar;

namespace Roller.Infrastructure.Domain;

[SugarTable]
public abstract class EntityBase<T> : IEntityBase<T> where T : IEquatable<T>
{
    [SugarColumn(IsPrimaryKey = true)] public T Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? Name { get; set; }

    public T CreatorId { get; set; }

    public T? UpdaterId { get; set; }

    public bool IsDeleted { get; set; }
}