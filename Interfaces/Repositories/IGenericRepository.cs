using ApiGMPKlik.Application.Interfaces;
using Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiGMPKlik.Interfaces.Repositories
{
    public interface IEntity<TId>
    {
        TId Id { get; set; }
    }

    /// <summary>
    /// Generic Repository interface dengan support soft delete dan audit trail
    /// Flexible: support BaseEntity, ApplicationUser, atau class yang implement IAuditable + ISoftDeletable
    /// </summary>
    public interface IGenericRepository<T, TId> where T : class, IEntity<TId>, IAuditable, ISoftDeletable
    {
        /// <summary>
        /// Queryable dengan global query filter (soft delete) aktif
        /// </summary>
        IQueryable<T> Query();

        /// <summary>
        /// Queryable dengan global query filter dinonaktifkan (untuk admin)
        /// </summary>
        IQueryable<T> QueryIgnoreFilters();

        /// <summary>
        /// Get entity by id
        /// </summary>
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get entity by id dengan include
        /// </summary>
        Task<T?> GetByIdAsync(TId id, Expression<Func<T, object>>[] includes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get entity by id dengan global filter dinonaktifkan
        /// </summary>
        Task<T?> GetByIdIgnoreFiltersAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get entity by predicate
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all entities dengan predicate
        /// </summary>
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add new entity
        /// </summary>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add multiple entities
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update entity
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Update multiple entities
        /// </summary>
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>
        /// Soft delete entity
        /// </summary>
        void SoftDelete(T entity, string? deletedBy);

        /// <summary>
        /// Soft delete by id
        /// </summary>
        Task<bool> SoftDeleteByIdAsync(TId id, string? deletedBy, CancellationToken cancellationToken = default);

        /// <summary>
        /// Hard delete entity (permanent)
        /// </summary>
        void HardDelete(T entity);

        /// <summary>
        /// Hard delete by id
        /// </summary>
        Task<bool> HardDeleteByIdAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Restore soft deleted entity
        /// </summary>
        void Restore(T entity, string? restoredBy);

        /// <summary>
        /// Check if entity exists
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Count entities
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if entity exists by id
        /// </summary>
        Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementasi Generic Repository dengan support soft delete dan query filtering
    /// </summary>
    public class GenericRepository<T, TId> : IGenericRepository<T, TId> where T : class, IEntity<TId>, IAuditable, ISoftDeletable
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        private readonly ICurrentUserService? _currentUserService;

        public GenericRepository(
            ApplicationDbContext context,
            ICurrentUserService? currentUserService = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            _currentUserService = currentUserService;
        }

        #region Query Methods

        /// <summary>
        /// Get queryable dengan global filter aktif (exclude soft deleted)
        /// </summary>
        public virtual IQueryable<T> Query()
        {
            return _dbSet.Where(e => !e.IsDeleted);
        }

        /// <summary>
        /// Get queryable tanpa filter (termasuk soft deleted) - untuk admin
        /// </summary>
        public virtual IQueryable<T> QueryIgnoreFilters()
        {
            return _dbSet;
        }

        #endregion

        #region Get Methods

        /// <summary>
        /// Get entity by id
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(
            TId id,
            CancellationToken cancellationToken = default)
        {
            return await Query()
                .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get entity by id dengan includes
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(
            TId id,
            Expression<Func<T, object>>[] includes,
            CancellationToken cancellationToken = default)
        {
            var query = Query();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query
                .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get entity by id tanpa filter global
        /// </summary>
        public virtual async Task<T?> GetByIdIgnoreFiltersAsync(
            TId id,
            CancellationToken cancellationToken = default)
        {
            return await QueryIgnoreFilters()
                .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get entity by predicate
        /// </summary>
        public virtual async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            return await Query()
                .FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get all entities dengan optional predicate
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            var query = Query();

            if (predicate != null)
                query = query.Where(predicate);

            return await query.ToListAsync(cancellationToken);
        }

        #endregion

        #region Add/Insert Methods

        /// <summary>
        /// Add single entity
        /// </summary>
        public virtual async Task<T> AddAsync(
            T entity,
            CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrEmpty(entity.CreatedBy))
            {
                entity.CreatedBy = _currentUserService?.UserId ?? "System";
                entity.CreatedAt = DateTime.UtcNow;
            }

            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Add multiple entities
        /// </summary>
        public virtual async Task AddRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default)
        {
            var entityList = entities.ToList();

            if (entityList.Count == 0)
                return;

            foreach (var entity in entityList)
            {
                if (string.IsNullOrEmpty(entity.CreatedBy))
                {
                    entity.CreatedBy = _currentUserService?.UserId ?? "System";
                    entity.CreatedAt = DateTime.UtcNow;
                }
            }

            await _dbSet.AddRangeAsync(entityList, cancellationToken);
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Update single entity
        /// </summary>
        public virtual void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.ModifiedAt == null)
            {
                entity.ModifiedAt = DateTime.UtcNow;
            }
            if (string.IsNullOrEmpty(entity.ModifiedBy))
            {
                entity.ModifiedBy = _currentUserService?.UserId ?? "System";
            }

            _dbSet.Update(entity);
        }

        /// <summary>
        /// Update multiple entities
        /// </summary>
        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            var entityList = entities.ToList();

            foreach (var entity in entityList)
            {
                if (entity.ModifiedAt == null)
                {
                    entity.ModifiedAt = DateTime.UtcNow;
                }
                if (string.IsNullOrEmpty(entity.ModifiedBy))
                {
                    entity.ModifiedBy = _currentUserService?.UserId ?? "System";
                }
            }

            _dbSet.UpdateRange(entityList);
        }

        #endregion

        #region Delete Methods

        /// <summary>
        /// Soft delete entity (mark as deleted, keep data)
        /// </summary>
        public virtual void SoftDelete(T entity, string? deletedBy = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var userId = deletedBy ?? _currentUserService?.UserId ?? "System";
            entity.SoftDelete(userId);

            _dbSet.Update(entity);
        }

        /// <summary>
        /// Soft delete by id
        /// </summary>
        public virtual async Task<bool> SoftDeleteByIdAsync(
            TId id,
            string? deletedBy = null,
            CancellationToken cancellationToken = default)
        {
            var entity = await QueryIgnoreFilters()
                .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken: cancellationToken);

            if (entity == null)
                return false;

            SoftDelete(entity, deletedBy);
            return true;
        }

        /// <summary>
        /// Hard delete entity (permanent delete from database)
        /// </summary>
        public virtual void HardDelete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Hard delete by id
        /// </summary>
        public virtual async Task<bool> HardDeleteByIdAsync(
            TId id,
            CancellationToken cancellationToken = default)
        {
            var entity = await QueryIgnoreFilters()
                .FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken: cancellationToken);

            if (entity == null)
                return false;

            HardDelete(entity);
            return true;
        }

        /// <summary>
        /// Restore soft deleted entity
        /// </summary>
        public virtual void Restore(T entity, string? restoredBy = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var userId = restoredBy ?? _currentUserService?.UserId ?? "System";
            entity.Restore(userId);

            _dbSet.Update(entity);
        }

        #endregion

        #region Existence Check Methods

        /// <summary>
        /// Check if entity exists dengan optional predicate
        /// </summary>
        public virtual async Task<bool> AnyAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            var query = Query();

            if (predicate == null)
                return await query.AnyAsync(cancellationToken);

            return await query.AnyAsync(predicate, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Count entities dengan optional predicate
        /// </summary>
        public virtual async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            var query = Query();

            if (predicate == null)
                return await query.CountAsync(cancellationToken);

            return await query.CountAsync(predicate, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Check if entity exists by id
        /// </summary>
        public virtual async Task<bool> ExistsAsync(
            TId id,
            CancellationToken cancellationToken = default)
        {
            return await Query()
                .AnyAsync(e => e.Id!.Equals(id), cancellationToken: cancellationToken);
        }

        #endregion
    }
}