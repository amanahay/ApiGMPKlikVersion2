using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiGMPKlik.Interfaces.Repositories;

public interface IRepository<TEntity, TContext> where TEntity : class where TContext : DbContext
{
    Task<TEntity?> GetByIdAsync(object id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);
    IQueryable<TEntity> AsQueryable();

}
public class Repository<TEntity, TContext> : IRepository<TEntity, TContext>
    where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(TContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate) =>
        await _dbSet.SingleOrDefaultAsync(predicate);

    public virtual async Task AddAsync(TEntity entity) => await _dbSet.AddAsync(entity);

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities) => await _dbSet.AddRangeAsync(entities);

    public virtual void Update(TEntity entity) => _dbSet.Update(entity);

    public virtual void Remove(TEntity entity) => _dbSet.Remove(entity);

    public virtual void RemoveRange(IEnumerable<TEntity> entities) => _dbSet.RemoveRange(entities);

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate) =>
        await _dbSet.AnyAsync(predicate);

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

    public virtual IQueryable<TEntity> AsQueryable() => _dbSet.AsQueryable();
}