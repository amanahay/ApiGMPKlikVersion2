using ApiGMPKlik.Demo;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Contexts;

public class SecondaryDbContext : DbContext
{
    public SecondaryDbContext(DbContextOptions<SecondaryDbContext> options) : base(options) { }

    // DbSets untuk data lokal, cache, atau sinkronisasi
    public DbSet<LocalCache> LocalCaches { get; set; }
    public DbSet<OfflineSyncQueue> OfflineSyncQueues { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // SQLite specific configurations
        builder.ApplyConfigurationsFromAssembly(typeof(SecondaryDbContext).Assembly);

        // Contoh: Index untuk performa
        builder.Entity<LocalCache>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.HasIndex(e => e.ExpiryDate);
        });
    }
}