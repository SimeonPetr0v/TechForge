using System.Linq.Expressions;

namespace TechForge.Core.Interfaces;

public interface IStore<T> where T : class
{
    Task<T?> GetByIdAsync(int id);

    Task<IReadOnlyList<T>> ListAllAsync();

    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate);

    IQueryable<T> Query();

    Task<T> AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);

    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    Task<int> SaveChangesAsync();
}
