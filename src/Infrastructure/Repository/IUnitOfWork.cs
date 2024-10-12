namespace Roller.Infrastructure.Repository;

public interface IUnitOfWork
{
    SqlSugarScope DbClient { get; }

    void BeginTransaction();

    void CommitTransaction();

    void RollbackTransaction();
}