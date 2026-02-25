using ApiGMPKlik.Application.Interfaces;
using ApiGMPKlik.DTOs.Address;
using ApiGMPKlik.Infrastructure.Address;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Interfaces.Repositories;
using ApiGMPKlik.Models.Entities.Address;
using ApiGMPKlik.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace ApiGMPKlik.Services.Address
{
    /// <summary>
    /// HELPER METHODS: Solusi clean code untuk menghindari type mismatch
    /// 
    /// MASALAH: 
    /// IQueryable + .Include() = IIncludableQueryable
    /// IQueryable + .OrderBy() = IOrderedQueryable
    /// Keduanya tidak bisa langsung di-assign ke variable yang lain tanpa casting
    /// 
    /// SOLUSI DENGAN ANALOGI:
    /// Seperti membuat adapter universal yang bisa menerima berbagai jenis IQueryable
    /// dan mengubahnya menjadi tipe "base" yang aman untuk semua operasi chaining
    /// </summary>
    public static class QueryHelper
    {
        /// <summary>
        /// Normalize IIncludableQueryable menjadi IQueryable base class
        /// Tujuan: Agar result dari Include bisa di-assign ke variable IQueryable
        /// </summary>
        public static IQueryable<T> AsBaseQueryable<T>(this IIncludableQueryable<T, object> query)
            where T : class
            => query.AsQueryable();

        /// <summary>
        /// Normalize untuk multiple entity type yang di-include
        /// </summary>
        public static IQueryable<T> AsBaseQueryable<T, TNavigationProperty>(
            this IIncludableQueryable<T, TNavigationProperty> query)
            where T : class
            where TNavigationProperty : class
            => query.AsQueryable();

        /// <summary>
        /// Ensure method chaining selalu aman tanpa type error
        /// </summary>
        public static IQueryable<T> ToQueryable<T>(this IQueryable<T> query)
            where T : class
            => query;

        public static IQueryable<T> ToQueryable<T>(this IOrderedQueryable<T> query)
            where T : class
            => query;
    }

    #region == PROVINSI SERVICE ==

    public class WilayahProvinsiService : IWilayahProvinsiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<WilayahProvinsi, int> _repository;
        private readonly ICurrentUserService _currentUserService;

        public WilayahProvinsiService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<WilayahProvinsi, int>();
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<ProvinsiReadDto>> CreateAsync(CreateProvinsiDto dto, CancellationToken cancellationToken = default)
        {
            var namaExists = await _repository.AnyAsync(e => e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<ProvinsiReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama Provinsi sudah ada" }
                });

            var kodeExists = await _repository.AnyAsync(e => e.KodeProvinsi == dto.KodeProvinsi, cancellationToken);
            if (kodeExists)
                return ApiResponse<ProvinsiReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "KodeProvinsi", Message = "Kode Provinsi sudah digunakan" }
                });

            var entity = new WilayahProvinsi
            {
                KodeProvinsi = dto.KodeProvinsi,
                Nama = dto.Nama,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Timezone = dto.Timezone,
                SortOrder = dto.SortOrder,
                Notes = dto.Notes
            };

            entity.MarkAsCreated(_currentUserService.UserId ?? "System");
            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<ProvinsiReadDto>.Created(MapToReadDto(entity), "Provinsi berhasil dibuat");
        }

        public async Task<ApiResponse<ProvinsiReadDto>> UpdateAsync(int id, UpdateProvinsiDto dto, CancellationToken cancellationToken = default)
        {
            if (id != dto.Id)
                return ApiResponse<ProvinsiReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Message = "ID tidak cocok" }
                });

            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<ProvinsiReadDto>.NotFound("Provinsi");

            var namaExists = await _repository.AnyAsync(
                e => e.Id != id && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<ProvinsiReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama Provinsi sudah digunakan oleh data lain" }
                });

            var kodeExists = await _repository.AnyAsync(
                e => e.Id != id && e.KodeProvinsi == dto.KodeProvinsi, cancellationToken);
            if (kodeExists)
                return ApiResponse<ProvinsiReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "KodeProvinsi", Message = "Kode Provinsi sudah digunakan oleh data lain" }
                });

            entity.KodeProvinsi = dto.KodeProvinsi;
            entity.Nama = dto.Nama;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.Timezone = dto.Timezone;
            entity.SortOrder = dto.SortOrder;
            entity.Notes = dto.Notes;

            entity.MarkAsUpdated(_currentUserService.UserId ?? "System");
            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<ProvinsiReadDto>.Success(MapToReadDto(entity), "Provinsi berhasil diupdate");
        }

        public async Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<object>.NotFound("Provinsi");

            var kotaRepo = _unitOfWork.Repository<WilayahKotaKab, int>();
            var hasChildren = await kotaRepo.AnyAsync(k => k.ProvinsiId == id, cancellationToken);

            if (hasChildren)
                return ApiResponse<object>.Error(
                    "Tidak dapat menghapus Provinsi",
                    "Provinsi masih memiliki Kota/Kabupaten. Hapus semua Kota/Kabupaten terlebih dahulu.",
                    400);

            _repository.SoftDelete(entity, _currentUserService.UserId ?? "System");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.Success(null!, "Provinsi berhasil dihapus");
        }

        public async Task<ApiResponse<ProvinsiReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<ProvinsiReadDto>.NotFound("Provinsi");

            return ApiResponse<ProvinsiReadDto>.Success(MapToReadDto(entity));
        }

        public async Task<ApiResponse<List<ProvinsiDropdownDto>>> GetDropdownAsync(CancellationToken cancellationToken = default)
        {
            var items = await _repository.Query()
                .OrderBy(e => e.SortOrder).ThenBy(e => e.Nama)
                .Select(e => new ProvinsiDropdownDto
                {
                    Id = e.Id,
                    KodeProvinsi = e.KodeProvinsi,
                    Nama = e.Nama
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<ProvinsiDropdownDto>>.Success(items);
        }

        public async Task<ApiResponse<List<ProvinsiListDto>>> GetListAsync(
            int page, int pageSize, string? keyword, string? sortBy, string? sortOrder,
            CancellationToken cancellationToken = default)
        {
            var query = _repository.Query();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e =>
                    e.Nama.ToLower().Contains(keyword) ||
                    e.KodeProvinsi.Contains(keyword));
            }

            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("nama", "desc") => query.OrderByDescending(e => e.Nama).ToQueryable(),
                ("nama", _) => query.OrderBy(e => e.Nama).ToQueryable(),
                ("kode", "desc") => query.OrderByDescending(e => e.KodeProvinsi).ToQueryable(),
                ("kode", _) => query.OrderBy(e => e.KodeProvinsi).ToQueryable(),
                ("sortorder", "desc") => query.OrderByDescending(e => e.SortOrder).ToQueryable(),
                _ => query.OrderBy(e => e.SortOrder).ThenBy(e => e.Nama).ToQueryable()
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ProvinsiListDto
                {
                    Id = e.Id,
                    KodeProvinsi = e.KodeProvinsi,
                    Nama = e.Nama,
                    SortOrder = e.SortOrder,
                    IsActive = e.IsActive,
                    KotaKabCount = e.KotaKabs.Count
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<ProvinsiListDto>>.Paginated(items, page, pageSize, totalCount);
        }

        private static ProvinsiReadDto MapToReadDto(WilayahProvinsi e)
        {
            return new ProvinsiReadDto
            {
                Id = e.Id,
                KodeProvinsi = e.KodeProvinsi,
                Nama = e.Nama,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Timezone = e.Timezone,
                SortOrder = e.SortOrder,
                Notes = e.Notes,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.ModifiedAt
            };
        }
    }

    #endregion

    #region == KOTA/KAB SERVICE ==

    public class WilayahKotaKabService : IWilayahKotaKabService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<WilayahKotaKab, int> _repository;
        private readonly ICurrentUserService _currentUserService;

        public WilayahKotaKabService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<WilayahKotaKab, int>();
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<KotaKabReadDto>> CreateAsync(CreateKotaKabDto dto, CancellationToken cancellationToken = default)
        {
            var provinsiRepo = _unitOfWork.Repository<WilayahProvinsi, int>();
            var provinsiExists = await provinsiRepo.AnyAsync(p => p.Id == dto.ProvinsiId, cancellationToken);
            if (!provinsiExists)
                return ApiResponse<KotaKabReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "ProvinsiId", Message = "Provinsi tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.ProvinsiId == dto.ProvinsiId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<KotaKabReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan di Provinsi ini" }
                });

            var entity = new WilayahKotaKab
            {
                ProvinsiId = dto.ProvinsiId,
                KodeKotaKabupaten = dto.KodeKotaKabupaten,
                Nama = dto.Nama,
                Jenis = dto.Jenis,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                SortOrder = dto.SortOrder,
                Notes = dto.Notes
            };

            entity.MarkAsCreated(_currentUserService.UserId ?? "System");
            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<KotaKabReadDto>.Created(await MapToReadDtoAsync(entity, cancellationToken), "Kota/Kabupaten berhasil dibuat");
        }

        public async Task<ApiResponse<KotaKabReadDto>> UpdateAsync(int id, UpdateKotaKabDto dto, CancellationToken cancellationToken = default)
        {
            if (id != dto.Id)
                return ApiResponse<KotaKabReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Message = "ID tidak cocok" }
                });

            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<KotaKabReadDto>.NotFound("Kota/Kabupaten");

            var provinsiRepo = _unitOfWork.Repository<WilayahProvinsi, int>();
            var provinsiExists = await provinsiRepo.AnyAsync(p => p.Id == dto.ProvinsiId, cancellationToken);
            if (!provinsiExists)
                return ApiResponse<KotaKabReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "ProvinsiId", Message = "Provinsi tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.Id != id && e.ProvinsiId == dto.ProvinsiId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<KotaKabReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan di Provinsi ini" }
                });

            entity.ProvinsiId = dto.ProvinsiId;
            entity.KodeKotaKabupaten = dto.KodeKotaKabupaten;
            entity.Nama = dto.Nama;
            entity.Jenis = dto.Jenis;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.SortOrder = dto.SortOrder;
            entity.Notes = dto.Notes;

            entity.MarkAsUpdated(_currentUserService.UserId ?? "System");
            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<KotaKabReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken), "Kota/Kabupaten berhasil diupdate");
        }

        public async Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<object>.NotFound("Kota/Kabupaten");

            var kecamatanRepo = _unitOfWork.Repository<WilayahKecamatan, int>();
            var hasChildren = await kecamatanRepo.AnyAsync(k => k.KotaKabupatenId == id, cancellationToken);

            if (hasChildren)
                return ApiResponse<object>.Error(
                    "Tidak dapat menghapus Kota/Kabupaten",
                    "Masih memiliki Kecamatan. Hapus semua Kecamatan terlebih dahulu.",
                    400);

            _repository.SoftDelete(entity, _currentUserService.UserId ?? "System");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.Success(null!, "Kota/Kabupaten berhasil dihapus");
        }

        public async Task<ApiResponse<KotaKabReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<KotaKabReadDto>.NotFound("Kota/Kabupaten");

            return ApiResponse<KotaKabReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken));
        }

        public async Task<ApiResponse<List<KotaKabDropdownDto>>> GetDropdownAsync(int? provinsiId = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query()
                 .Include(e => e.Provinsi)
                 .AsBaseQueryable();

            if (provinsiId.HasValue)
                query = query.Where(e => e.ProvinsiId == provinsiId.Value);
            var items = await query
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Nama)
                .Select(e => new KotaKabDropdownDto
                {
                    Id = e.Id,
                    KodeKotaKabupaten = e.KodeKotaKabupaten,
                    Nama = e.Nama,
                    Jenis = e.Jenis,
                    ProvinsiId = e.ProvinsiId
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<KotaKabDropdownDto>>.Success(items);
        }

        public async Task<ApiResponse<List<KotaKabListDto>>> GetListAsync(
            int page, int pageSize, string? keyword, string? sortBy, string? sortOrder,
            int? provinsiId = null, CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Gunakan var, bukan IIncludableQueryable - type auto-adjust sesuai operasi
            var query = _repository.Query().Include(e => e.Provinsi).AsBaseQueryable();

            if (provinsiId.HasValue)
                query = query.Where(e => e.ProvinsiId == provinsiId.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e =>
                    e.Nama.ToLower().Contains(keyword) ||
                    e.KodeKotaKabupaten.Contains(keyword) ||
                    e.Provinsi.Nama.ToLower().Contains(keyword));
            }

            // ✅ FIX: Gunakan .ToQueryable() untuk normalize OrderBy result
            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("nama", "desc") => query.OrderByDescending(e => e.Nama).ToQueryable(),
                ("nama", _) => query.OrderBy(e => e.Nama).ToQueryable(),
                ("kode", "desc") => query.OrderByDescending(e => e.KodeKotaKabupaten).ToQueryable(),
                ("kode", _) => query.OrderBy(e => e.KodeKotaKabupaten).ToQueryable(),
                ("jenis", "desc") => query.OrderByDescending(e => e.Jenis).ToQueryable(),
                ("jenis", _) => query.OrderBy(e => e.Jenis).ToQueryable(),
                ("provinsi", "desc") => query.OrderByDescending(e => e.Provinsi.Nama).ToQueryable(),
                ("provinsi", _) => query.OrderBy(e => e.Provinsi.Nama).ToQueryable(),
                ("sortorder", "desc") => query.OrderByDescending(e => e.SortOrder).ToQueryable(),
                _ => query.OrderBy(e => e.SortOrder).ThenBy(e => e.Nama).ToQueryable()
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new KotaKabListDto
                {
                    Id = e.Id,
                    KodeKotaKabupaten = e.KodeKotaKabupaten,
                    Nama = e.Nama,
                    Jenis = e.Jenis,
                    SortOrder = e.SortOrder,
                    IsActive = e.IsActive,
                    ProvinsiNama = e.Provinsi.Nama,
                    KecamatanCount = e.Kecamatans.Count
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<KotaKabListDto>>.Paginated(items, page, pageSize, totalCount);
        }

        public async Task<ApiResponse<List<KotaKabDropdownDto>>> GetByProvinsiIdAsync(int provinsiId, CancellationToken cancellationToken = default)
        {
            return await GetDropdownAsync(provinsiId, cancellationToken);
        }

        private async Task<KotaKabReadDto> MapToReadDtoAsync(WilayahKotaKab e, CancellationToken cancellationToken = default)
        {
            var provinsi = await _unitOfWork.Repository<WilayahProvinsi, int>()
                .GetByIdAsync(e.ProvinsiId, cancellationToken);

            return new KotaKabReadDto
            {
                Id = e.Id,
                KodeKotaKabupaten = e.KodeKotaKabupaten,
                Nama = e.Nama,
                Jenis = e.Jenis,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                SortOrder = e.SortOrder,
                Notes = e.Notes,
                IsActive = e.IsActive,
                ProvinsiId = e.ProvinsiId,
                ProvinsiNama = provinsi?.Nama ?? "",
                ProvinsiKode = provinsi?.KodeProvinsi ?? ""
            };
        }
    }

    #endregion

    #region == KECAMATAN SERVICE ==

    public class WilayahKecamatanService : IWilayahKecamatanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<WilayahKecamatan, int> _repository;
        private readonly ICurrentUserService _currentUserService;

        public WilayahKecamatanService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<WilayahKecamatan, int>();
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<KecamatanReadDto>> CreateAsync(CreateKecamatanDto dto, CancellationToken cancellationToken = default)
        {
            var kotaRepo = _unitOfWork.Repository<WilayahKotaKab, int>();
            var kotaExists = await kotaRepo.AnyAsync(k => k.Id == dto.KotaKabupatenId, cancellationToken);
            if (!kotaExists)
                return ApiResponse<KecamatanReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "KotaKabupatenId", Message = "Kota/Kabupaten tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.KotaKabupatenId == dto.KotaKabupatenId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<KecamatanReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            var entity = new WilayahKecamatan
            {
                KotaKabupatenId = dto.KotaKabupatenId,
                KodeKecamatan = dto.KodeKecamatan,
                Nama = dto.Nama,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                SortOrder = dto.SortOrder,
                Notes = dto.Notes
            };

            entity.MarkAsCreated(_currentUserService.UserId ?? "System");
            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<KecamatanReadDto>.Created(await MapToReadDtoAsync(entity, cancellationToken), "Kecamatan berhasil dibuat");
        }

        public async Task<ApiResponse<KecamatanReadDto>> UpdateAsync(int id, UpdateKecamatanDto dto, CancellationToken cancellationToken = default)
        {
            if (id != dto.Id)
                return ApiResponse<KecamatanReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Message = "ID tidak cocok" }
                });

            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<KecamatanReadDto>.NotFound("Kecamatan");

            var kotaRepo = _unitOfWork.Repository<WilayahKotaKab, int>();
            var kotaExists = await kotaRepo.AnyAsync(k => k.Id == dto.KotaKabupatenId, cancellationToken);
            if (!kotaExists)
                return ApiResponse<KecamatanReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "KotaKabupatenId", Message = "Kota/Kabupaten tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.Id != id && e.KotaKabupatenId == dto.KotaKabupatenId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<KecamatanReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            entity.KotaKabupatenId = dto.KotaKabupatenId;
            entity.KodeKecamatan = dto.KodeKecamatan;
            entity.Nama = dto.Nama;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.SortOrder = dto.SortOrder;
            entity.Notes = dto.Notes;

            entity.MarkAsUpdated(_currentUserService.UserId ?? "System");
            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<KecamatanReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken), "Kecamatan berhasil diupdate");
        }

        public async Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<object>.NotFound("Kecamatan");

            var kelurahanRepo = _unitOfWork.Repository<WilayahKelurahanDesa, int>();
            var hasChildren = await kelurahanRepo.AnyAsync(k => k.KecamatanId == id, cancellationToken);

            if (hasChildren)
                return ApiResponse<object>.Error(
                    "Tidak dapat menghapus Kecamatan",
                    "Masih memiliki Kelurahan/Desa. Hapus semua terlebih dahulu.",
                    400);

            _repository.SoftDelete(entity, _currentUserService.UserId ?? "System");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.Success(null!, "Kecamatan berhasil dihapus");
        }

        public async Task<ApiResponse<KecamatanReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<KecamatanReadDto>.NotFound("Kecamatan");

            return ApiResponse<KecamatanReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken));
        }

        public async Task<ApiResponse<List<KecamatanDropdownDto>>> GetDropdownAsync(int? kotaKabId = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query().Include(e => e.KotaKab).AsBaseQueryable();

            if (kotaKabId.HasValue)
                query = query.Where(e => e.KotaKabupatenId == kotaKabId.Value);

            var items = await query
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Nama)
                .Select(e => new KecamatanDropdownDto
                {
                    Id = e.Id,
                    KodeKecamatan = e.KodeKecamatan,
                    Nama = e.Nama,
                    KotaKabupatenId = e.KotaKabupatenId
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<KecamatanDropdownDto>>.Success(items);
        }

        public async Task<ApiResponse<List<KecamatanListDto>>> GetListAsync(
            int page, int pageSize, string? keyword, string? sortBy, string? sortOrder,
            int? kotaKabId = null, CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Gunakan var + Include + AsBaseQueryable
            var query = _repository.Query()
                .Include(e => e.KotaKab)
                .ThenInclude(k => k.Provinsi)
                .AsBaseQueryable();

            if (kotaKabId.HasValue)
                query = query.Where(e => e.KotaKabupatenId == kotaKabId.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e =>
                    e.Nama.ToLower().Contains(keyword) ||
                    e.KodeKecamatan.Contains(keyword));
            }

            // ✅ FIX: Gunakan .ToQueryable() untuk normalize OrderBy
            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("nama", "desc") => query.OrderByDescending(e => e.Nama).ToQueryable(),
                ("nama", _) => query.OrderBy(e => e.Nama).ToQueryable(),
                ("kode", "desc") => query.OrderByDescending(e => e.KodeKecamatan).ToQueryable(),
                ("kode", _) => query.OrderBy(e => e.KodeKecamatan).ToQueryable(),
                ("sortorder", "desc") => query.OrderByDescending(e => e.SortOrder).ToQueryable(),
                _ => query.OrderBy(e => e.SortOrder).ThenBy(e => e.Nama).ToQueryable()
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new KecamatanListDto
                {
                    Id = e.Id,
                    KodeKecamatan = e.KodeKecamatan,
                    Nama = e.Nama,
                    SortOrder = e.SortOrder,
                    IsActive = e.IsActive,
                    KotaKabNama = e.KotaKab.Nama,
                    ProvinsiNama = e.KotaKab.Provinsi.Nama,
                    KelurahanDesaCount = e.KelurahanDesas.Count
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<KecamatanListDto>>.Paginated(items, page, pageSize, totalCount);
        }

        public async Task<ApiResponse<List<KecamatanDropdownDto>>> GetByKotaKabIdAsync(int kotaKabId, CancellationToken cancellationToken = default)
        {
            return await GetDropdownAsync(kotaKabId, cancellationToken);
        }

        private async Task<KecamatanReadDto> MapToReadDtoAsync(WilayahKecamatan e, CancellationToken cancellationToken = default)
        {
            var kotaRepo = _unitOfWork.Repository<WilayahKotaKab, int>();
            var kota = await kotaRepo.Query()
                .Include(k => k.Provinsi)
                .FirstOrDefaultAsync(k => k.Id == e.KotaKabupatenId, cancellationToken);

            return new KecamatanReadDto
            {
                Id = e.Id,
                KodeKecamatan = e.KodeKecamatan,
                Nama = e.Nama,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                SortOrder = e.SortOrder,
                Notes = e.Notes,
                IsActive = e.IsActive,
                KotaKabupatenId = e.KotaKabupatenId,
                KotaKabNama = kota?.Nama ?? "",
                ProvinsiNama = kota?.Provinsi?.Nama ?? ""
            };
        }
    }

    #endregion

    #region == KELURAHAN/DESA SERVICE ==

    public class WilayahKelurahanDesaService : IWilayahKelurahanDesaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<WilayahKelurahanDesa, int> _repository;
        private readonly ICurrentUserService _currentUserService;

        public WilayahKelurahanDesaService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<WilayahKelurahanDesa, int>();
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<KelurahanDesaReadDto>> CreateAsync(CreateKelurahanDesaDto dto, CancellationToken cancellationToken = default)
        {
            var kecamatanRepo = _unitOfWork.Repository<WilayahKecamatan, int>();
            var kecamatanExists = await kecamatanRepo.AnyAsync(k => k.Id == dto.KecamatanId, cancellationToken);
            if (!kecamatanExists)
                return ApiResponse<KelurahanDesaReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "KecamatanId", Message = "Kecamatan tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.KecamatanId == dto.KecamatanId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<KelurahanDesaReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            var entity = new WilayahKelurahanDesa
            {
                KecamatanId = dto.KecamatanId,
                KodeKelurahanDesa = dto.KodeKelurahanDesa,
                Nama = dto.Nama,
                Jenis = dto.Jenis,
                KodePos = dto.KodePos,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                SortOrder = dto.SortOrder,
                Notes = dto.Notes
            };

            entity.MarkAsCreated(_currentUserService.UserId ?? "System");
            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<KelurahanDesaReadDto>.Created(await MapToReadDtoAsync(entity, cancellationToken), "Kelurahan/Desa berhasil dibuat");
        }

        public async Task<ApiResponse<KelurahanDesaReadDto>> UpdateAsync(int id, UpdateKelurahanDesaDto dto, CancellationToken cancellationToken = default)
        {
            if (id != dto.Id)
                return ApiResponse<KelurahanDesaReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Message = "ID tidak cocok" }
                });

            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<KelurahanDesaReadDto>.NotFound("Kelurahan/Desa");

            var kecamatanRepo = _unitOfWork.Repository<WilayahKecamatan, int>();
            var kecamatanExists = await kecamatanRepo.AnyAsync(k => k.Id == dto.KecamatanId, cancellationToken);
            if (!kecamatanExists)
                return ApiResponse<KelurahanDesaReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "KecamatanId", Message = "Kecamatan tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.Id != id && e.KecamatanId == dto.KecamatanId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<KelurahanDesaReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            entity.KecamatanId = dto.KecamatanId;
            entity.KodeKelurahanDesa = dto.KodeKelurahanDesa;
            entity.Nama = dto.Nama;
            entity.Jenis = dto.Jenis;
            entity.KodePos = dto.KodePos;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.SortOrder = dto.SortOrder;
            entity.Notes = dto.Notes;

            entity.MarkAsUpdated(_currentUserService.UserId ?? "System");
            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<KelurahanDesaReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken), "Kelurahan/Desa berhasil diupdate");
        }

        public async Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<object>.NotFound("Kelurahan/Desa");

            var dusunRepo = _unitOfWork.Repository<WilayahDusun, int>();
            var hasChildren = await dusunRepo.AnyAsync(d => d.KelurahanDesaId == id, cancellationToken);

            if (hasChildren)
                return ApiResponse<object>.Error(
                    "Tidak dapat menghapus Kelurahan/Desa",
                    "Masih memiliki Dusun. Hapus semua terlebih dahulu.",
                    400);

            _repository.SoftDelete(entity, _currentUserService.UserId ?? "System");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.Success(null!, "Kelurahan/Desa berhasil dihapus");
        }

        public async Task<ApiResponse<KelurahanDesaReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<KelurahanDesaReadDto>.NotFound("Kelurahan/Desa");

            return ApiResponse<KelurahanDesaReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken));
        }

        public async Task<ApiResponse<List<KelurahanDesaDropdownDto>>> GetDropdownAsync(int? kecamatanId = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query();

            if (kecamatanId.HasValue)
                query = query.Where(e => e.KecamatanId == kecamatanId.Value);

            var items = await query
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Nama)
                .Select(e => new KelurahanDesaDropdownDto
                {
                    Id = e.Id,
                    KodeKelurahanDesa = e.KodeKelurahanDesa,
                    Nama = e.Nama,
                    Jenis = e.Jenis,
                    KecamatanId = e.KecamatanId
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<KelurahanDesaDropdownDto>>.Success(items);
        }

        public async Task<ApiResponse<List<KelurahanDesaListDto>>> GetListAsync(
            int page, int pageSize, string? keyword, string? sortBy, string? sortOrder,
            int? kecamatanId = null, CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Gunakan var + ThenInclude + AsBaseQueryable
            var query = _repository.Query()
                .Include(e => e.Kecamatan)
                .ThenInclude(k => k.KotaKab)
                .AsBaseQueryable();

            if (kecamatanId.HasValue)
                query = query.Where(e => e.KecamatanId == kecamatanId.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e =>
                    e.Nama.ToLower().Contains(keyword) ||
                    e.KodeKelurahanDesa.Contains(keyword));
            }

            // ✅ FIX: Gunakan .ToQueryable()
            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("nama", "desc") => query.OrderByDescending(e => e.Nama).ToQueryable(),
                ("nama", _) => query.OrderBy(e => e.Nama).ToQueryable(),
                ("kode", "desc") => query.OrderByDescending(e => e.KodeKelurahanDesa).ToQueryable(),
                ("kode", _) => query.OrderBy(e => e.KodeKelurahanDesa).ToQueryable(),
                ("jenis", "desc") => query.OrderByDescending(e => e.Jenis).ToQueryable(),
                ("jenis", _) => query.OrderBy(e => e.Jenis).ToQueryable(),
                ("sortorder", "desc") => query.OrderByDescending(e => e.SortOrder).ToQueryable(),
                _ => query.OrderBy(e => e.SortOrder).ThenBy(e => e.Nama).ToQueryable()
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new KelurahanDesaListDto
                {
                    Id = e.Id,
                    KodeKelurahanDesa = e.KodeKelurahanDesa,
                    Nama = e.Nama,
                    Jenis = e.Jenis,
                    KodePos = e.KodePos,
                    SortOrder = e.SortOrder,
                    IsActive = e.IsActive,
                    KecamatanNama = e.Kecamatan.Nama,
                    KotaKabNama = e.Kecamatan.KotaKab.Nama,
                    DusunCount = e.Dusuns.Count
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<KelurahanDesaListDto>>.Paginated(items, page, pageSize, totalCount);
        }

        public async Task<ApiResponse<List<KelurahanDesaDropdownDto>>> GetByKecamatanIdAsync(int kecamatanId, CancellationToken cancellationToken = default)
        {
            return await GetDropdownAsync(kecamatanId, cancellationToken);
        }

        private async Task<KelurahanDesaReadDto> MapToReadDtoAsync(WilayahKelurahanDesa e, CancellationToken cancellationToken = default)
        {
            var kecamatanRepo = _unitOfWork.Repository<WilayahKecamatan, int>();
            var kecamatan = await kecamatanRepo.Query()
                .Include(k => k.KotaKab)
                .FirstOrDefaultAsync(k => k.Id == e.KecamatanId, cancellationToken);

            return new KelurahanDesaReadDto
            {
                Id = e.Id,
                KodeKelurahanDesa = e.KodeKelurahanDesa,
                Nama = e.Nama,
                Jenis = e.Jenis,
                KodePos = e.KodePos,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                SortOrder = e.SortOrder,
                Notes = e.Notes,
                IsActive = e.IsActive,
                KecamatanId = e.KecamatanId,
                KecamatanNama = kecamatan?.Nama ?? "",
                KotaKabNama = kecamatan?.KotaKab?.Nama ?? "",
                ProvinsiNama = kecamatan?.KotaKab?.Provinsi?.Nama ?? ""
            };
        }
    }

    #endregion

    #region == DUSUN SERVICE ==

    public class WilayahDusunService : IWilayahDusunService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<WilayahDusun, int> _repository;
        private readonly ICurrentUserService _currentUserService;

        public WilayahDusunService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<WilayahDusun, int>();
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<DusunReadDto>> CreateAsync(CreateDusunDto dto, CancellationToken cancellationToken = default)
        {
            var kelurahanRepo = _unitOfWork.Repository<WilayahKelurahanDesa, int>();
            var kelurahanExists = await kelurahanRepo.AnyAsync(k => k.Id == dto.KelurahanDesaId, cancellationToken);
            if (!kelurahanExists)
                return ApiResponse<DusunReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "KelurahanDesaId", Message = "Kelurahan/Desa tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.KelurahanDesaId == dto.KelurahanDesaId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<DusunReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            var entity = new WilayahDusun
            {
                KelurahanDesaId = dto.KelurahanDesaId,
                Nama = dto.Nama,
                SortOrder = dto.SortOrder,
                Notes = dto.Notes
            };

            entity.MarkAsCreated(_currentUserService.UserId ?? "System");
            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<DusunReadDto>.Created(await MapToReadDtoAsync(entity, cancellationToken), "Dusun berhasil dibuat");
        }

        public async Task<ApiResponse<DusunReadDto>> UpdateAsync(int id, UpdateDusunDto dto, CancellationToken cancellationToken = default)
        {
            if (id != dto.Id)
                return ApiResponse<DusunReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Message = "ID tidak cocok" }
                });

            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<DusunReadDto>.NotFound("Dusun");

            var kelurahanRepo = _unitOfWork.Repository<WilayahKelurahanDesa, int>();
            var kelurahanExists = await kelurahanRepo.AnyAsync(k => k.Id == dto.KelurahanDesaId, cancellationToken);
            if (!kelurahanExists)
                return ApiResponse<DusunReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "KelurahanDesaId", Message = "Kelurahan/Desa tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.Id != id && e.KelurahanDesaId == dto.KelurahanDesaId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<DusunReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            entity.KelurahanDesaId = dto.KelurahanDesaId;
            entity.Nama = dto.Nama;
            entity.SortOrder = dto.SortOrder;
            entity.Notes = dto.Notes;

            entity.MarkAsUpdated(_currentUserService.UserId ?? "System");
            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<DusunReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken), "Dusun berhasil diupdate");
        }

        public async Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<object>.NotFound("Dusun");

            var rwRepo = _unitOfWork.Repository<WilayahRw, int>();
            var hasChildren = await rwRepo.AnyAsync(r => r.DusunId == id, cancellationToken);

            if (hasChildren)
                return ApiResponse<object>.Error(
                    "Tidak dapat menghapus Dusun",
                    "Masih memiliki RW. Hapus semua terlebih dahulu.",
                    400);

            _repository.SoftDelete(entity, _currentUserService.UserId ?? "System");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.Success(null!, "Dusun berhasil dihapus");
        }

        public async Task<ApiResponse<DusunReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<DusunReadDto>.NotFound("Dusun");

            return ApiResponse<DusunReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken));
        }

        public async Task<ApiResponse<List<DusunDropdownDto>>> GetDropdownAsync(int? kelurahanDesaId = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query();

            if (kelurahanDesaId.HasValue)
                query = query.Where(e => e.KelurahanDesaId == kelurahanDesaId.Value);

            var items = await query
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Nama)
                .Select(e => new DusunDropdownDto
                {
                    Id = e.Id,
                    Nama = e.Nama,
                    KelurahanDesaId = e.KelurahanDesaId
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<DusunDropdownDto>>.Success(items);
        }

        public async Task<ApiResponse<List<DusunListDto>>> GetListAsync(
            int page, int pageSize, string? keyword, string? sortBy, string? sortOrder,
            int? kelurahanDesaId = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query()
                .Include(e => e.KelurahanDesa)
                .ThenInclude(k => k.Kecamatan)
                .AsBaseQueryable();

            if (kelurahanDesaId.HasValue)
                query = query.Where(e => e.KelurahanDesaId == kelurahanDesaId.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e => e.Nama.ToLower().Contains(keyword));
            }

            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("nama", "desc") => query.OrderByDescending(e => e.Nama).ToQueryable(),
                ("nama", _) => query.OrderBy(e => e.Nama).ToQueryable(),
                ("sortorder", "desc") => query.OrderByDescending(e => e.SortOrder).ToQueryable(),
                _ => query.OrderBy(e => e.SortOrder).ThenBy(e => e.Nama).ToQueryable()
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new DusunListDto
                {
                    Id = e.Id,
                    Nama = e.Nama,
                    SortOrder = e.SortOrder,
                    IsActive = e.IsActive,
                    KelurahanDesaNama = e.KelurahanDesa.Nama,
                    KecamatanNama = e.KelurahanDesa.Kecamatan.Nama,
                    RwCount = e.Rws.Count
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<DusunListDto>>.Paginated(items, page, pageSize, totalCount);
        }

        public async Task<ApiResponse<List<DusunDropdownDto>>> GetByKelurahanDesaIdAsync(int kelurahanDesaId, CancellationToken cancellationToken = default)
        {
            return await GetDropdownAsync(kelurahanDesaId, cancellationToken);
        }

        private async Task<DusunReadDto> MapToReadDtoAsync(WilayahDusun e, CancellationToken cancellationToken = default)
        {
            var kelurahanRepo = _unitOfWork.Repository<WilayahKelurahanDesa, int>();
            var kelurahan = await kelurahanRepo.Query()
                .Include(k => k.Kecamatan)
                .FirstOrDefaultAsync(k => k.Id == e.KelurahanDesaId, cancellationToken);

            return new DusunReadDto
            {
                Id = e.Id,
                Nama = e.Nama,
                SortOrder = e.SortOrder,
                Notes = e.Notes,
                IsActive = e.IsActive,
                KelurahanDesaId = e.KelurahanDesaId,
                KelurahanDesaNama = kelurahan?.Nama ?? "",
                KecamatanNama = kelurahan?.Kecamatan?.Nama ?? ""
            };
        }
    }

    #endregion

    #region == RW SERVICE ==

    public class WilayahRwService : IWilayahRwService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<WilayahRw, int> _repository;
        private readonly ICurrentUserService _currentUserService;

        public WilayahRwService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<WilayahRw, int>();
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<RwReadDto>> CreateAsync(CreateRwDto dto, CancellationToken cancellationToken = default)
        {
            var dusunRepo = _unitOfWork.Repository<WilayahDusun, int>();
            var dusunExists = await dusunRepo.AnyAsync(d => d.Id == dto.DusunId, cancellationToken);
            if (!dusunExists)
                return ApiResponse<RwReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "DusunId", Message = "Dusun tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.DusunId == dto.DusunId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<RwReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            var entity = new WilayahRw
            {
                DusunId = dto.DusunId,
                Nama = dto.Nama,
                SortOrder = dto.SortOrder
            };

            entity.MarkAsCreated(_currentUserService.UserId ?? "System");
            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<RwReadDto>.Created(await MapToReadDtoAsync(entity, cancellationToken), "RW berhasil dibuat");
        }

        public async Task<ApiResponse<RwReadDto>> UpdateAsync(int id, UpdateRwDto dto, CancellationToken cancellationToken = default)
        {
            if (id != dto.Id)
                return ApiResponse<RwReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Message = "ID tidak cocok" }
                });

            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<RwReadDto>.NotFound("RW");

            var dusunRepo = _unitOfWork.Repository<WilayahDusun, int>();
            var dusunExists = await dusunRepo.AnyAsync(d => d.Id == dto.DusunId, cancellationToken);
            if (!dusunExists)
                return ApiResponse<RwReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "DusunId", Message = "Dusun tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.Id != id && e.DusunId == dto.DusunId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<RwReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            entity.DusunId = dto.DusunId;
            entity.Nama = dto.Nama;
            entity.SortOrder = dto.SortOrder;

            entity.MarkAsUpdated(_currentUserService.UserId ?? "System");
            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<RwReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken), "RW berhasil diupdate");
        }

        public async Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<object>.NotFound("RW");

            var rtRepo = _unitOfWork.Repository<WilayahRt, int>();
            var hasChildren = await rtRepo.AnyAsync(r => r.RwId == id, cancellationToken);

            if (hasChildren)
                return ApiResponse<object>.Error(
                    "Tidak dapat menghapus RW",
                    "Masih memiliki RT. Hapus semua terlebih dahulu.",
                    400);

            _repository.SoftDelete(entity, _currentUserService.UserId ?? "System");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.Success(null!, "RW berhasil dihapus");
        }

        public async Task<ApiResponse<RwReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<RwReadDto>.NotFound("RW");

            return ApiResponse<RwReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken));
        }

        public async Task<ApiResponse<List<RwDropdownDto>>> GetDropdownAsync(int? dusunId = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query();

            if (dusunId.HasValue)
                query = query.Where(e => e.DusunId == dusunId.Value);

            var items = await query
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Nama)
                .Select(e => new RwDropdownDto
                {
                    Id = e.Id,
                    Nama = e.Nama,
                    DusunId = e.DusunId
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<RwDropdownDto>>.Success(items);
        }

        public async Task<ApiResponse<List<RwListDto>>> GetListAsync(
            int page, int pageSize, string? keyword, string? sortBy, string? sortOrder,
            int? dusunId = null, CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Gunakan var + ThenInclude + AsBaseQueryable
            var query = _repository.Query()
                .Include(e => e.Dusun)
                .ThenInclude(d => d.KelurahanDesa)
                .AsBaseQueryable();

            if (dusunId.HasValue)
                query = query.Where(e => e.DusunId == dusunId.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e => e.Nama.ToLower().Contains(keyword));
            }

            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("nama", "desc") => query.OrderByDescending(e => e.Nama).ToQueryable(),
                ("nama", _) => query.OrderBy(e => e.Nama).ToQueryable(),
                ("sortorder", "desc") => query.OrderByDescending(e => e.SortOrder).ToQueryable(),
                _ => query.OrderBy(e => e.SortOrder).ThenBy(e => e.Nama).ToQueryable()
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new RwListDto
                {
                    Id = e.Id,
                    Nama = e.Nama,
                    SortOrder = e.SortOrder,
                    IsActive = e.IsActive,
                    DusunNama = e.Dusun.Nama,
                    KelurahanDesaNama = e.Dusun.KelurahanDesa.Nama,
                    RtCount = e.Rts.Count
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<RwListDto>>.Paginated(items, page, pageSize, totalCount);
        }

        public async Task<ApiResponse<List<RwDropdownDto>>> GetByDusunIdAsync(int dusunId, CancellationToken cancellationToken = default)
        {
            return await GetDropdownAsync(dusunId, cancellationToken);
        }

        private async Task<RwReadDto> MapToReadDtoAsync(WilayahRw e, CancellationToken cancellationToken = default)
        {
            var dusunRepo = _unitOfWork.Repository<WilayahDusun, int>();
            var dusun = await dusunRepo.Query()
                .Include(d => d.KelurahanDesa)
                .FirstOrDefaultAsync(d => d.Id == e.DusunId, cancellationToken);

            return new RwReadDto
            {
                Id = e.Id,
                Nama = e.Nama,
                SortOrder = e.SortOrder,
                IsActive = e.IsActive,
                DusunId = e.DusunId,
                DusunNama = dusun?.Nama ?? "",
                KelurahanDesaNama = dusun?.KelurahanDesa?.Nama ?? ""
            };
        }
    }

    #endregion

    #region == RT SERVICE ==

    public class WilayahRtService : IWilayahRtService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<WilayahRt, int> _repository;
        private readonly ICurrentUserService _currentUserService;

        public WilayahRtService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _repository = unitOfWork.Repository<WilayahRt, int>();
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<RtReadDto>> CreateAsync(CreateRtDto dto, CancellationToken cancellationToken = default)
        {
            var rwRepo = _unitOfWork.Repository<WilayahRw, int>();
            var rwExists = await rwRepo.AnyAsync(r => r.Id == dto.RwId, cancellationToken);
            if (!rwExists)
                return ApiResponse<RtReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "RwId", Message = "RW tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.RwId == dto.RwId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<RtReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            var entity = new WilayahRt
            {
                RwId = dto.RwId,
                Nama = dto.Nama,
                SortOrder = dto.SortOrder
            };

            entity.MarkAsCreated(_currentUserService.UserId ?? "System");
            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<RtReadDto>.Created(await MapToReadDtoAsync(entity, cancellationToken), "RT berhasil dibuat");
        }

        public async Task<ApiResponse<RtReadDto>> UpdateAsync(int id, UpdateRtDto dto, CancellationToken cancellationToken = default)
        {
            if (id != dto.Id)
                return ApiResponse<RtReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Message = "ID tidak cocok" }
                });

            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<RtReadDto>.NotFound("RT");

            var rwRepo = _unitOfWork.Repository<WilayahRw, int>();
            var rwExists = await rwRepo.AnyAsync(r => r.Id == dto.RwId, cancellationToken);
            if (!rwExists)
                return ApiResponse<RtReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "RwId", Message = "RW tidak ditemukan" }
                });

            var namaExists = await _repository.AnyAsync(
                e => e.Id != id && e.RwId == dto.RwId && e.Nama.ToLower() == dto.Nama.ToLower(), cancellationToken);
            if (namaExists)
                return ApiResponse<RtReadDto>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "Nama", Message = "Nama sudah digunakan" }
                });

            entity.RwId = dto.RwId;
            entity.Nama = dto.Nama;
            entity.SortOrder = dto.SortOrder;

            entity.MarkAsUpdated(_currentUserService.UserId ?? "System");
            _repository.Update(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<RtReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken), "RT berhasil diupdate");
        }

        public async Task<ApiResponse<object>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<object>.NotFound("RT");

            _repository.SoftDelete(entity, _currentUserService.UserId ?? "System");
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.Success(null!, "RT berhasil dihapus");
        }

        public async Task<ApiResponse<RtReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return ApiResponse<RtReadDto>.NotFound("RT");

            return ApiResponse<RtReadDto>.Success(await MapToReadDtoAsync(entity, cancellationToken));
        }

        public async Task<ApiResponse<List<RtDropdownDto>>> GetDropdownAsync(int? rwId = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.Query();

            if (rwId.HasValue)
                query = query.Where(e => e.RwId == rwId.Value);

            var items = await query
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Nama)
                .Select(e => new RtDropdownDto
                {
                    Id = e.Id,
                    Nama = e.Nama,
                    RwId = e.RwId
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<RtDropdownDto>>.Success(items);
        }

        public async Task<ApiResponse<List<RtListDto>>> GetListAsync(
            int page, int pageSize, string? keyword, string? sortBy, string? sortOrder,
            int? rwId = null, CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Gunakan var + multiple ThenInclude + AsBaseQueryable
            var query = _repository.Query()
                .Include(e => e.Rw)
                .ThenInclude(r => r.Dusun)
                .ThenInclude(d => d.KelurahanDesa)
                .AsBaseQueryable();

            if (rwId.HasValue)
                query = query.Where(e => e.RwId == rwId.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(e => e.Nama.ToLower().Contains(keyword));
            }

            query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
            {
                ("nama", "desc") => query.OrderByDescending(e => e.Nama).ToQueryable(),
                ("nama", _) => query.OrderBy(e => e.Nama).ToQueryable(),
                ("sortorder", "desc") => query.OrderByDescending(e => e.SortOrder).ToQueryable(),
                _ => query.OrderBy(e => e.SortOrder).ThenBy(e => e.Nama).ToQueryable()
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new RtListDto
                {
                    Id = e.Id,
                    Nama = e.Nama,
                    SortOrder = e.SortOrder,
                    IsActive = e.IsActive,
                    RwNama = e.Rw.Nama,
                    DusunNama = e.Rw.Dusun.Nama,
                    KelurahanDesaNama = e.Rw.Dusun.KelurahanDesa.Nama
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<RtListDto>>.Paginated(items, page, pageSize, totalCount);
        }

        public async Task<ApiResponse<List<RtDropdownDto>>> GetByRwIdAsync(int rwId, CancellationToken cancellationToken = default)
        {
            return await GetDropdownAsync(rwId, cancellationToken);
        }

        private async Task<RtReadDto> MapToReadDtoAsync(WilayahRt e, CancellationToken cancellationToken = default)
        {
            var fullAddress = "";
            if (e.Rw?.Dusun?.KelurahanDesa?.Kecamatan?.KotaKab?.Provinsi != null)
            {
                var k = e.Rw.Dusun.KelurahanDesa.Kecamatan.KotaKab.Provinsi;
                fullAddress = $"{k.Nama}, {e.Rw.Dusun.KelurahanDesa.Kecamatan.KotaKab.Nama}, {e.Rw.Dusun.KelurahanDesa.Kecamatan.Nama}, {e.Rw.Dusun.KelurahanDesa.Nama}, Dusun {e.Rw.Dusun.Nama}, RW {e.Rw.Nama}, RT {e.Nama}";
            }

            return new RtReadDto
            {
                Id = e.Id,
                Nama = e.Nama,
                SortOrder = e.SortOrder,
                IsActive = e.IsActive,
                RwId = e.RwId,
                RwNama = e.Rw?.Nama ?? "",
                DusunNama = e.Rw?.Dusun?.Nama ?? "",
                KelurahanDesaNama = e.Rw?.Dusun?.KelurahanDesa?.Nama ?? "",
                FullAddress = fullAddress
            };
        }
    }

    #endregion

    #region == ADDRESS SEARCH SERVICE ==

    public class AddressSearchService : IAddressSearchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddressSearchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<AddressSearchResultDto>>> SearchAsync(string keyword, int limit = 7, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                return ApiResponse<List<AddressSearchResultDto>>.ValidationError(new List<ErrorDetail> {
                    new ErrorDetail { Field = "keyword", Message = "Keyword minimal 2 karakter" }
                });

            keyword = keyword.ToLower();
            var results = new List<AddressSearchResultDto>();
            var remaining = limit;

            // 1. Cari Kelurahan/Desa dulu (Level 4)
            var kelurahanRepo = _unitOfWork.Repository<WilayahKelurahanDesa, int>();
            var kelurahanResults = await kelurahanRepo.Query()
                .Where(e => EF.Functions.Like(e.Nama.ToLower(), $"%{keyword}%") ||
                           EF.Functions.Like(e.KodeKelurahanDesa, $"%{keyword}%"))
                .OrderBy(e => e.Nama)
                .Take(remaining)
                .Include(e => e.Kecamatan).ThenInclude(k => k.KotaKab).ThenInclude(k => k.Provinsi)
                .Select(e => new AddressSearchResultDto
                {
                    Id = e.Id,
                    Type = e.Jenis == "Desa" ? "Desa" : "Kelurahan",
                    Code = e.KodeKelurahanDesa,
                    Name = e.Nama,
                    FullPath = $"{e.Kecamatan.KotaKab.Provinsi.Nama} > {e.Kecamatan.KotaKab.Jenis} {e.Kecamatan.KotaKab.Nama} > Kec. {e.Kecamatan.Nama} > {e.Jenis} {e.Nama}",
                    ParentId = e.KecamatanId,
                    ParentName = e.Kecamatan.Nama,
                    Level = 4
                })
                .ToListAsync(cancellationToken);

            results.AddRange(kelurahanResults);
            remaining = limit - results.Count;

            // 2. Jika masih kurang, cari Kecamatan (Level 3)
            if (remaining > 0)
            {
                var existingIds = kelurahanResults.Select(r => r.ParentId).Distinct().ToList();
                var kecamatanRepo = _unitOfWork.Repository<WilayahKecamatan, int>();
                var kecamatanResults = await kecamatanRepo.Query()
                    .Where(e => !existingIds.Contains(e.Id) &&
                               (EF.Functions.Like(e.Nama.ToLower(), $"%{keyword}%") ||
                                EF.Functions.Like(e.KodeKecamatan, $"%{keyword}%")))
                    .OrderBy(e => e.Nama)
                    .Take(remaining)
                    .Include(e => e.KotaKab).ThenInclude(k => k.Provinsi)
                    .Select(e => new AddressSearchResultDto
                    {
                        Id = e.Id,
                        Type = "Kecamatan",
                        Code = e.KodeKecamatan,
                        Name = e.Nama,
                        FullPath = $"{e.KotaKab.Provinsi.Nama} > {e.KotaKab.Jenis} {e.KotaKab.Nama} > Kec. {e.Nama}",
                        ParentId = e.KotaKabupatenId,
                        ParentName = e.KotaKab.Nama,
                        Level = 3
                    })
                    .ToListAsync(cancellationToken);

                results.AddRange(kecamatanResults);
                remaining = limit - results.Count;
            }

            // 3. Jika masih kurang, cari Kota/Kab (Level 2)
            if (remaining > 0)
            {
                var kotaRepo = _unitOfWork.Repository<WilayahKotaKab, int>();
                var kotaResults = await kotaRepo.Query()
                    .Where(e => EF.Functions.Like(e.Nama.ToLower(), $"%{keyword}%") ||
                               EF.Functions.Like(e.KodeKotaKabupaten, $"%{keyword}%"))
                    .OrderBy(e => e.Nama)
                    .Take(remaining)
                    .Include(e => e.Provinsi)
                    .Select(e => new AddressSearchResultDto
                    {
                        Id = e.Id,
                        Type = e.Jenis,
                        Code = e.KodeKotaKabupaten,
                        Name = e.Nama,
                        FullPath = $"{e.Provinsi.Nama} > {e.Jenis} {e.Nama}",
                        ParentId = e.ProvinsiId,
                        ParentName = e.Provinsi.Nama,
                        Level = 2
                    })
                    .ToListAsync(cancellationToken);

                results.AddRange(kotaResults);
                remaining = limit - results.Count;
            }

            // 4. Jika masih kurang, cari Provinsi (Level 1)
            if (remaining > 0)
            {
                var provinsiRepo = _unitOfWork.Repository<WilayahProvinsi, int>();
                var provinsiResults = await provinsiRepo.Query()
                    .Where(e => EF.Functions.Like(e.Nama.ToLower(), $"%{keyword}%") ||
                               EF.Functions.Like(e.KodeProvinsi, $"%{keyword}%"))
                    .OrderBy(e => e.Nama)
                    .Take(remaining)
                    .Select(e => new AddressSearchResultDto
                    {
                        Id = e.Id,
                        Type = "Provinsi",
                        Code = e.KodeProvinsi,
                        Name = e.Nama,
                        FullPath = e.Nama,
                        ParentId = null,
                        ParentName = null,
                        Level = 1
                    })
                    .ToListAsync(cancellationToken);

                results.AddRange(provinsiResults);
            }

            // Sort by relevance
            results = results
                .OrderByDescending(r => r.Name.ToLower() == keyword)
                .ThenByDescending(r => r.Name.ToLower().StartsWith(keyword))
                .ThenBy(r => r.Name)
                .Take(limit)
                .ToList();

            return ApiResponse<List<AddressSearchResultDto>>.Success(results,
                results.Count > 0 ? "Alamat ditemukan" : "Tidak ada hasil");
        }
    }

    #endregion
}