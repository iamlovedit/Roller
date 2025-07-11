﻿using System.Linq.Expressions;
using SqlSugar;

namespace Roller.Infrastructure.Repository
{
    public abstract class ServiceBase<T, TId>(IRepositoryBase<T, TId> dbContext)
        : IServiceBase<T, TId> where T : class, new() where TId : IEquatable<TId>
    {
        public IRepositoryBase<T, TId> DAL { get; } = dbContext;

        public async Task<T?> GetByIdAsync(TId id)
        {
            return await DAL.GetByIdAsync(id);
        }

        public async Task<long> AddSnowflakeAsync(T entity)
        {
            return await DAL.AddSnowflakeAsync(entity);
        }

        public async Task<T?> AddEntityAsync(T entity)
        {
            return await DAL.AddEntityAsync(entity);
        }

        public async Task<IList<long>> AddSnowflakesAsync(IList<T> entities)
        {
            return await DAL.AddSnowflakesAsync(entities);
        }

        public async Task<PageData<T>?> QueryPageAsync(Expression<Func<T, bool>>? whereExpression, int pageIndex = 1,
            int pageSize = 20,
            Expression<Func<T, object>>? orderExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            return await DAL.QueryPageAsync(whereExpression, pageIndex, pageSize, orderExpression, orderByType);
        }

        public async Task<List<T>?> GetAllAsync()
        {
            return await DAL.GetAllAsync();
        }

        public async Task<List<T>?> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await DAL.FindAsync(predicate);
        }

        public async Task<T?> GetFirstByExpressionAsync(Expression<Func<T, bool>> expression)
        {
            return await DAL.GetFirstByExpressionAsync(expression);
        }

        public async Task<bool> UpdateColumnsAsync(T entity, Expression<Func<T, object>> expression)
        {
            return await DAL.UpdateColumnsAsync(entity, expression);
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            return await DAL.UpdateAsync(entity);
        }

        public async Task<bool> DeleteByIdAsync(long id)
        {
            return await DAL.DeleteByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            return await DAL.DeleteAsync(entity);
        }

        public async Task<bool> DeletedByExpressionAsync(Expression<Func<T, bool>> expression)
        {
            return await DAL.DeletedByExpressionAsync(expression);
        }
    }
}