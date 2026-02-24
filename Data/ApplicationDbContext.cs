using ApiGMPKlik.Demo;
using ApiGMPKlik.Models;  // PASTIKAN INI ADA
using ApiGMPKlik.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserSecurity> UserSecurities { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<ReferralTree> ReferralTrees { get; set; }
        public DbSet<ApiClient> ApiClients { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserSecurityQuestion> UserSecurityQuestions { get; set; }
        public DbSet<MasterSecurityQuestion> MasterSecurityQuestions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("dbo");

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users", "identity");
                entity.Property(e => e.FullName).HasMaxLength(200);
                entity.HasIndex(e => e.IsActive);

                // INI YANG ERROR - PASTIKAN Property ada di class ApplicationUser
                entity.HasIndex(e => e.BranchId);
                entity.HasOne(u => u.Branch)
                      .WithMany(b => b.Users)
                      .HasForeignKey(u => u.BranchId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ApplicationRole>(entity => entity.ToTable("Roles", "identity"));
            builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles", "identity"));
            builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UserClaims", "identity"));
            builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins", "identity"));
            builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaims", "identity"));
            builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UserTokens", "identity"));

            builder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("UserProfiles", "identity");
                entity.HasIndex(e => e.UserId).IsUnique();

                // FIX: Decimal precision untuk financial data
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.Property(e => e.Commission).HasPrecision(18, 2);
                entity.Property(e => e.MonthlyBudget).HasPrecision(18, 2);
                entity.Property(e => e.UsedBudgetThisMonth).HasPrecision(18, 2);

                // Property lengths
                entity.Property(e => e.Gender).HasMaxLength(50);
                entity.Property(e => e.BirthPlace).HasMaxLength(255);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Facebook).HasMaxLength(255);
                entity.Property(e => e.Instagram).HasMaxLength(255);
                entity.Property(e => e.Twitter).HasMaxLength(255);
                entity.Property(e => e.Youtube).HasMaxLength(255);
                entity.Property(e => e.Linkedin).HasMaxLength(255);
                entity.Property(e => e.Telegram).HasMaxLength(255);
                entity.Property(e => e.Website).HasMaxLength(255);
                entity.Property(e => e.TaxId).HasMaxLength(50);
                entity.Property(e => e.BankName).HasMaxLength(100);
                entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
                entity.Property(e => e.HeirName).HasMaxLength(200);
                entity.Property(e => e.HeirPhone).HasMaxLength(20);
                entity.Property(e => e.Language).HasMaxLength(10).HasDefaultValue("id");
                entity.Property(e => e.TimeZone).HasMaxLength(50);
                entity.Property(e => e.Locale).HasMaxLength(20);

                entity.HasOne(e => e.User)
                      .WithOne(u => u.Profile)
                      .HasForeignKey<UserProfile>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserSecurity>(entity =>
            {
                entity.ToTable("UserSecurities", "identity");
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasOne(e => e.User)
                      .WithOne(u => u.Security)
                      .HasForeignKey<UserSecurity>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Branch>(entity =>
            {
                entity.ToTable("Branches", "dbo");
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.City);

                // FIX: Decimal precision untuk koordinat GPS
                entity.Property(e => e.Latitude).HasPrecision(10, 8);
                entity.Property(e => e.Longitude).HasPrecision(11, 8);
            });

            builder.Entity<ReferralTree>(entity =>
            {
                entity.ToTable("ReferralTrees", "dbo");
                entity.HasIndex(e => new { e.RootUserId, e.Level });
                entity.HasIndex(e => new { e.ReferredUserId, e.Level }).IsUnique();
                entity.HasIndex(e => e.ParentUserId);
                entity.HasIndex(e => e.IsActive);

                entity.HasOne(e => e.RootUser)
                      .WithMany(u => u.ReferralAncestors)
                      .HasForeignKey(e => e.RootUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReferredUser)
                      .WithMany(u => u.ReferralDescendants)
                      .HasForeignKey(e => e.ReferredUserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ParentUser)
                      .WithMany()
                      .HasForeignKey(e => e.ParentUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ApiClient>(entity =>
            {
                entity.ToTable("ApiClients", "dbo");
                entity.HasIndex(e => e.ApiKeyPrefix).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ExpiresAt);

                entity.Property(e => e.ApiKeyHash).HasMaxLength(500);
                entity.Property(e => e.ApiKeyPrefix).HasMaxLength(100);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Permission configuration
            builder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions", "dbo");
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Module);

                entity.Property(e => e.Code).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Module).HasMaxLength(50);
            });

            builder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions", "dbo");
                entity.HasKey(e => new { e.RoleId, e.PermissionId });

                entity.HasOne(e => e.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(e => e.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserSecurityQuestion>(entity =>
            {
                entity.ToTable("UserSecurityQuestions", "identity");
                entity.HasIndex(e => e.UserId);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.SecurityQuestions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Question).HasMaxLength(500);
                entity.Property(e => e.AnswerHash).HasMaxLength(500); // Hashed answer
            });
            builder.Entity<MasterSecurityQuestion>(entity =>
            {
                entity.ToTable("MasterSecurityQuestions", "identity");
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.SortOrder);
            });
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}