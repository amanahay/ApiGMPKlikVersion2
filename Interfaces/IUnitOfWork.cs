using ApiGMPKlik.Application.Interfaces;
using ApiGMPKlik.Interfaces.Repositories;
using Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ApiGMPKlik.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Get repository untuk entity T yang implement IAuditable dan ISoftDeletable
        /// Support BaseEntity, ApplicationUser, atau class lain yang implement interfaces
        /// </summary>
        IGenericRepository<T, TId> Repository<T, TId>() where T : class, IEntity<TId>, IAuditable, ISoftDeletable;

        /// <summary>
        /// Save changes ke database dengan audit trail
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin transaction baru
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commit transaction
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback transaction
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if ada transaction yang aktif
        /// </summary>
        bool HasActiveTransaction { get; }
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;
        private Dictionary<Type, object>? _repositories;
        private readonly ICurrentUserService? _currentUserService;

        public UnitOfWork(ApplicationDbContext context, ICurrentUserService? currentUserService = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new Dictionary<Type, object>();
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Get atau create repository untuk entity T dengan lazy loading
        /// Constraint HARUS SAMA dengan interface: where T : class, IAuditable, ISoftDeletable
        /// </summary>
        public IGenericRepository<T, TId> Repository<T, TId>() where T : class, IEntity<TId>, IAuditable, ISoftDeletable
        {
            if (_repositories == null)
                _repositories = new Dictionary<Type, object>();

            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<T, TId>(_context, _currentUserService);
                _repositories.Add(type, repositoryInstance);
            }

            return (IGenericRepository<T, TId>)_repositories[type]!;
        }

        /// <summary>
        /// Save changes dengan automatic audit trail setting
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Set audit fields sebelum save
            SetAuditFields();

            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Begin transaction baru
        /// </summary>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("A transaction is already in progress");

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Commit transaction
        /// </summary>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No transaction in progress");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        /// <summary>
        /// Rollback transaction
        /// </summary>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                return;

            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public bool HasActiveTransaction => _currentTransaction != null;

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Set audit fields secara otomatis untuk semua entities sebelum save
        /// Support BaseEntity dan entity yang implement IAuditable + ISoftDeletable
        /// </summary>
        private void SetAuditFields()
        {
            var userId = _currentUserService?.UserId ?? "System";

            // Handle BaseEntity entries
            var baseEntries = _context.ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in baseEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.MarkAsCreated(userId);
                        break;

                    case EntityState.Modified:
                        entry.Entity.MarkAsUpdated(userId);
                        break;

                    case EntityState.Deleted:
                        // Convert hard delete ke soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.SoftDelete(userId);
                        break;
                }
            }

            // Handle IAuditable entries (untuk ApplicationUser dan entities lain)
            var auditableEntries = _context.ChangeTracker.Entries<IAuditable>();
            foreach (var entry in auditableEntries)
            {
                // Skip jika sudah dihandle sebagai BaseEntity
                if (entry.Entity is BaseEntity)
                    continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.MarkAsCreated(userId);
                        break;

                    case EntityState.Modified:
                        entry.Entity.MarkAsUpdated(userId);
                        break;
                }
            }

            // Handle ISoftDeletable entries (untuk soft delete)
            var softDeletableEntries = _context.ChangeTracker.Entries<ISoftDeletable>();
            foreach (var entry in softDeletableEntries)
            {
                // Skip jika sudah dihandle
                if (entry.Entity is BaseEntity || entry.Entity is IAuditable)
                    continue;

                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.SoftDelete(userId);
                }
            }
        }
    }
}