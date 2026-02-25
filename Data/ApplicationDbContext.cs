using ApiGMPKlik.Demo;
using ApiGMPKlik.Models;  // PASTIKAN INI ADA
using ApiGMPKlik.Models.DataPrice;
using ApiGMPKlik.Models.Entities;
using ApiGMPKlik.Models.Entities.Address;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        public DbSet<WilayahProvinsi> WilayahProvinsis { get; set; }
        public DbSet<WilayahKotaKab> WilayahKotaKabs { get; set; }
        public DbSet<WilayahKecamatan> WilayahKecamatans { get; set; }
        public DbSet<WilayahKelurahanDesa> WilayahKelurahanDesas { get; set; }
        public DbSet<WilayahDusun> WilayahDusuns { get; set; }
        public DbSet<WilayahRw> WilayahRws { get; set; }
        public DbSet<WilayahRt> WilayahRts { get; set; }

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

    public class WilayahProvinsiConfiguration : IEntityTypeConfiguration<WilayahProvinsi>
    {
        public void Configure(EntityTypeBuilder<WilayahProvinsi> builder)
        {
            // ✅ .NET Core 10.0 pattern: Check constraints via ToTable overload
            builder.ToTable("Wilayah_Provinsi", "dbo", t =>
            {
                t.HasCheckConstraint("CK_Provinsi_KodeLength", "LEN([kode_provinsi]) = 2");
                t.HasCheckConstraint("CK_Provinsi_Latitude", "[latitude] >= -90 AND [latitude] <= 90");
                t.HasCheckConstraint("CK_Provinsi_Longitude", "[longitude] >= -180 AND [longitude] <= 180");
            });

            builder.Property(e => e.KodeProvinsi)
                .HasColumnName("kode_provinsi")
                .HasMaxLength(2)
                .IsRequired();

            builder.Property(e => e.Nama)
                .HasColumnName("nama")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Latitude)
                .HasColumnName("latitude")
                .HasPrecision(10, 8);

            builder.Property(e => e.Longitude)
                .HasColumnName("longitude")
                .HasPrecision(11, 8);

            builder.Property(e => e.Timezone)
                .HasColumnName("timezone")
                .HasMaxLength(50);

            builder.Property(e => e.SortOrder)
                .HasColumnName("sort_order")
                .HasDefaultValue(0);

            builder.Property(e => e.Notes)
                .HasColumnName("notes");

            // Map ModifiedAt/By to UpdatedAt/By columns
            builder.Property(e => e.ModifiedAt).HasColumnName("UpdatedAt");
            builder.Property(e => e.ModifiedBy).HasColumnName("UpdatedBy");

            // Unique constraint: Nama tidak boleh duplikat di tabel Provinsi
            builder.HasIndex(e => e.Nama).IsUnique();
            builder.HasIndex(e => e.KodeProvinsi).IsUnique();

            // Soft delete query filter
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    public class WilayahKotaKabConfiguration : IEntityTypeConfiguration<WilayahKotaKab>
    {
        public void Configure(EntityTypeBuilder<WilayahKotaKab> builder)
        {
            // ✅ .NET Core 10.0 pattern
            builder.ToTable("Wilayah_KotaKab", "dbo", t =>
            {
                t.HasCheckConstraint("CK_KotaKab_Jenis", "[jenis] IN ('Kabupaten', 'Kota')");
                t.HasCheckConstraint("CK_KotaKab_KodeLength", "LEN([kode_kota_kabupaten]) = 4");
                t.HasCheckConstraint("CK_KotaKab_Latitude", "[latitude] >= -90 AND [latitude] <= 90");
                t.HasCheckConstraint("CK_KotaKab_Longitude", "[longitude] >= -180 AND [longitude] <= 180");
            });

            builder.Property(e => e.ProvinsiId).HasColumnName("provinsi_id");
            builder.Property(e => e.KodeKotaKabupaten).HasColumnName("kode_kota_kabupaten").HasMaxLength(4).IsRequired();
            builder.Property(e => e.Nama).HasColumnName("nama").HasMaxLength(255).IsRequired();
            builder.Property(e => e.Jenis).HasColumnName("jenis").HasMaxLength(20).IsRequired();
            builder.Property(e => e.Latitude).HasColumnName("latitude").HasPrecision(10, 8);
            builder.Property(e => e.Longitude).HasColumnName("longitude").HasPrecision(11, 8);
            builder.Property(e => e.SortOrder).HasColumnName("sort_order");
            builder.Property(e => e.Notes).HasColumnName("notes");

            builder.Property(e => e.ModifiedAt).HasColumnName("UpdatedAt");
            builder.Property(e => e.ModifiedBy).HasColumnName("UpdatedBy");

            // Composite Unique: Nama tidak boleh sama dalam satu Provinsi
            builder.HasIndex(e => new { e.ProvinsiId, e.Nama }).IsUnique();
            builder.HasIndex(e => e.KodeKotaKabupaten).IsUnique();

            // Foreign Key dengan Restrict (tidak bisa hapus provinsi jika ada kota)
            builder.HasOne(e => e.Provinsi)
                .WithMany(p => p.KotaKabs)
                .HasForeignKey(e => e.ProvinsiId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    public class WilayahKecamatanConfiguration : IEntityTypeConfiguration<WilayahKecamatan>
    {
        public void Configure(EntityTypeBuilder<WilayahKecamatan> builder)
        {
            // ✅ .NET Core 10.0 pattern
            builder.ToTable("Wilayah_Kecamatan", "dbo", t =>
            {
                t.HasCheckConstraint("CK_Kecamatan_KodeLength", "LEN([kode_kecamatan]) = 7");
                t.HasCheckConstraint("CK_Kecamatan_Latitude", "[latitude] >= -90 AND [latitude] <= 90");
                t.HasCheckConstraint("CK_Kecamatan_Longitude", "[longitude] >= -180 AND [longitude] <= 180");
            });

            builder.Property(e => e.KotaKabupatenId).HasColumnName("kota_kabupaten_id");
            builder.Property(e => e.KodeKecamatan).HasColumnName("kode_kecamatan").HasMaxLength(7).IsRequired();
            builder.Property(e => e.Nama).HasColumnName("nama").HasMaxLength(255).IsRequired();
            builder.Property(e => e.Latitude).HasColumnName("latitude").HasPrecision(10, 8);
            builder.Property(e => e.Longitude).HasColumnName("longitude").HasPrecision(11, 8);
            builder.Property(e => e.SortOrder).HasColumnName("sort_order");
            builder.Property(e => e.Notes).HasColumnName("notes");

            builder.Property(e => e.ModifiedAt).HasColumnName("UpdatedAt");
            builder.Property(e => e.ModifiedBy).HasColumnName("UpdatedBy");

            builder.HasIndex(e => new { e.KotaKabupatenId, e.Nama }).IsUnique();
            builder.HasIndex(e => e.KodeKecamatan).IsUnique();

            builder.HasOne(e => e.KotaKab)
                .WithMany(k => k.Kecamatans)
                .HasForeignKey(e => e.KotaKabupatenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    public class WilayahKelurahanDesaConfiguration : IEntityTypeConfiguration<WilayahKelurahanDesa>
    {
        public void Configure(EntityTypeBuilder<WilayahKelurahanDesa> builder)
        {
            // ✅ .NET Core 10.0 pattern
            builder.ToTable("Wilayah_KelurahanDesa", "dbo", t =>
            {
                t.HasCheckConstraint("CK_KelurahanDesa_Jenis", "[jenis] IN ('Desa', 'Kelurahan')");
                t.HasCheckConstraint("CK_KelurahanDesa_KodeLength", "LEN([kode_kelurahan_desa]) = 10");
                t.HasCheckConstraint("CK_KelurahanDesa_KodePos", "[kode_pos] IS NULL OR LEN([kode_pos]) = 5");
                t.HasCheckConstraint("CK_KelurahanDesa_Latitude", "[latitude] >= -90 AND [latitude] <= 90");
                t.HasCheckConstraint("CK_KelurahanDesa_Longitude", "[longitude] >= -180 AND [longitude] <= 180");
            });

            builder.Property(e => e.KecamatanId).HasColumnName("kecamatan_id");
            builder.Property(e => e.KodeKelurahanDesa).HasColumnName("kode_kelurahan_desa").HasMaxLength(10).IsRequired();
            builder.Property(e => e.Nama).HasColumnName("nama").HasMaxLength(255).IsRequired();
            builder.Property(e => e.Jenis).HasColumnName("jenis").HasMaxLength(20).IsRequired();
            builder.Property(e => e.KodePos).HasColumnName("kode_pos").HasMaxLength(10);
            builder.Property(e => e.Latitude).HasColumnName("latitude").HasPrecision(10, 8);
            builder.Property(e => e.Longitude).HasColumnName("longitude").HasPrecision(11, 8);
            builder.Property(e => e.SortOrder).HasColumnName("sort_order");
            builder.Property(e => e.Notes).HasColumnName("notes");

            builder.Property(e => e.ModifiedAt).HasColumnName("UpdatedAt");
            builder.Property(e => e.ModifiedBy).HasColumnName("UpdatedBy");

            builder.HasIndex(e => new { e.KecamatanId, e.Nama }).IsUnique();
            builder.HasIndex(e => e.KodeKelurahanDesa).IsUnique();

            builder.HasOne(e => e.Kecamatan)
                .WithMany(k => k.KelurahanDesas)
                .HasForeignKey(e => e.KecamatanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    public class WilayahDusunConfiguration : IEntityTypeConfiguration<WilayahDusun>
    {
        public void Configure(EntityTypeBuilder<WilayahDusun> builder)
        {
            builder.ToTable("Wilayah_Dusun", "dbo");

            builder.Property(e => e.KelurahanDesaId).HasColumnName("kelurahan_desa_id");
            builder.Property(e => e.Nama).HasColumnName("nama").HasMaxLength(255).IsRequired();
            builder.Property(e => e.SortOrder).HasColumnName("sort_order");
            builder.Property(e => e.Notes).HasColumnName("notes");

            builder.Property(e => e.ModifiedAt).HasColumnName("UpdatedAt");
            builder.Property(e => e.ModifiedBy).HasColumnName("UpdatedBy");

            builder.HasIndex(e => new { e.KelurahanDesaId, e.Nama }).IsUnique();

            builder.HasOne(e => e.KelurahanDesa)
                .WithMany(k => k.Dusuns)
                .HasForeignKey(e => e.KelurahanDesaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    public class WilayahRwConfiguration : IEntityTypeConfiguration<WilayahRw>
    {
        public void Configure(EntityTypeBuilder<WilayahRw> builder)
        {
            // ✅ .NET Core 10.0 pattern
            builder.ToTable("Wilayah_RW", "dbo", t =>
            {
                t.HasCheckConstraint("CK_Rw_Nama_Numeric", "ISNUMERIC([nama]) = 1");
                t.HasCheckConstraint("CK_Rw_Nama_Range", "CONVERT(int, [nama]) >= 1 AND CONVERT(int, [nama]) <= 999");
            });

            builder.Property(e => e.DusunId).HasColumnName("dusun_id");
            builder.Property(e => e.Nama).HasColumnName("nama").HasMaxLength(10).IsRequired();
            builder.Property(e => e.SortOrder).HasColumnName("sort_order");

            builder.Property(e => e.ModifiedAt).HasColumnName("UpdatedAt");
            builder.Property(e => e.ModifiedBy).HasColumnName("UpdatedBy");

            builder.HasIndex(e => new { e.DusunId, e.Nama }).IsUnique();

            builder.HasOne(e => e.Dusun)
                .WithMany(d => d.Rws)
                .HasForeignKey(e => e.DusunId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    public class WilayahRtConfiguration : IEntityTypeConfiguration<WilayahRt>
    {
        public void Configure(EntityTypeBuilder<WilayahRt> builder)
        {
            // ✅ .NET Core 10.0 pattern
            builder.ToTable("Wilayah_RT", "dbo", t =>
            {
                t.HasCheckConstraint("CK_Rt_Nama_Numeric", "ISNUMERIC([nama]) = 1");
                t.HasCheckConstraint("CK_Rt_Nama_Range", "CONVERT(int, [nama]) >= 1 AND CONVERT(int, [nama]) <= 999");
            });

            builder.Property(e => e.RwId).HasColumnName("rw_id");
            builder.Property(e => e.Nama).HasColumnName("nama").HasMaxLength(10).IsRequired();
            builder.Property(e => e.SortOrder).HasColumnName("sort_order");

            builder.Property(e => e.ModifiedAt).HasColumnName("UpdatedAt");
            builder.Property(e => e.ModifiedBy).HasColumnName("UpdatedBy");

            builder.HasIndex(e => new { e.RwId, e.Nama }).IsUnique();

            builder.HasOne(e => e.Rw)
                .WithMany(r => r.Rts)
                .HasForeignKey(e => e.RwId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
    public class DataPriceRangeConfiguration : IEntityTypeConfiguration<DataPriceRange>
    {
        public void Configure(EntityTypeBuilder<DataPriceRange> builder)
        {
            // ✅ .NET Core 10.0 pattern: ToTable dengan lambda untuk check constraints
            builder.ToTable("DataPriceRanges", "dbo", t =>
            {
                t.HasCheckConstraint("CK_DataPriceRange_MinPrice", "[MinPrice] >= 0");
                t.HasCheckConstraint("CK_DataPriceRange_PriceRange", "[MaxPrice] > [MinPrice]");
            });

            builder.Property(e => e.MinPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(e => e.MaxPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("IDR")
                .IsRequired();

            builder.Property(e => e.Color).HasMaxLength(7);
            builder.Property(e => e.Icon).HasMaxLength(50);
            builder.Property(e => e.Category).HasMaxLength(50);

            builder.Property(e => e.OrganizationId);

            builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
            builder.Property(e => e.Code).HasMaxLength(50);
            builder.Property(e => e.Description).HasMaxLength(1000);
            builder.Property(e => e.SortOrder).HasDefaultValue(0);

            // Map BaseEntity audit fields (ModifiedAt -> UpdatedAt)
            builder.Property(e => e.ModifiedAt).HasColumnName("UpdatedAt");
            builder.Property(e => e.ModifiedBy).HasColumnName("UpdatedBy");

            // Concurrency token
            builder.Property(e => e.RowVersion)
                .IsRowVersion()
                .HasColumnName("RowVersion");

            // Indexes - Hapus TenantId, tambah index untuk Name saja
            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => new { e.IsActive, e.SortOrder });
            builder.HasIndex(e => e.Code);

            // Soft delete query filter
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}