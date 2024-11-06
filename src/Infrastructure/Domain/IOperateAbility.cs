namespace Roller.Infrastructure.Domain;

public interface IOperateAbility<T> where T : IEquatable<T>
{
    T CreatorId { get; set; }

    T? UpdaterId { get; set; }
}