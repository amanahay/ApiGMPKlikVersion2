using ApiGMPKlik.Interfaces.Repositories;
using Infrastructure.Data.Contexts;

namespace ApiGMPKlik.Interfaces
{
    public interface ISecondaryUnitOfWork : IDisposable
    {
        IRepository<TEntity, SecondaryDbContext> Repository<TEntity>() where TEntity : class;
        Task<int> CommitAsync();
    }
    public class SecondaryUnitOfWork : ISecondaryUnitOfWork
    {
        private readonly SecondaryDbContext _context;
        private Dictionary<Type, object>? _repositories;

        public SecondaryUnitOfWork(SecondaryDbContext context)
        {
            _context = context;
        }

        public IRepository<TEntity, SecondaryDbContext> Repository<TEntity>() where TEntity : class
        {
            _repositories ??= new Dictionary<Type, object>();

            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new Repository<TEntity, SecondaryDbContext>(_context);
                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<TEntity, SecondaryDbContext>)_repositories[type]!;
        }

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
