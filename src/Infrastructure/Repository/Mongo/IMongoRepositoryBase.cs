﻿using System.Linq.Expressions;
using MongoDB.Driver;

namespace Roller.Infrastructure.Repository.Mongo;

public interface IMongoRepositoryBase<TEntity, in TKey>
    where TEntity : class, IIdentifiable, new() where TKey : IEquatable<TKey>
{
    IMongoCollection<TEntity> Collection { get; }

    Task AddAsync(TEntity entity);

    Task<TEntity?> GetAsync(TKey id);

    Task<List<TEntity>?> GetListAsync();

    Task<TEntity?> GetByObjectIdAsync(string id);

    Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    public Task<List<TEntity>?> GetListAsync(Expression<Func<TEntity, bool>> predicate);

    Task<List<TEntity>?> GetListFilterAsync(FilterDefinition<TEntity> filter);

    Task UpdateAsync(TKey id, TEntity entity);

    Task DeleteAsync(TKey id);

    Task<PageData<TEntity>?> GetPageDataAsync(int page, int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = false
    );
}