using ApiGMPKlik.Application.Interfaces;
using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Interfaces.Repositories;
using ApiGMPKlik.Models;
using ApiGMPKlik.Shared;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiGMPKlik.Services
{
    /// <summary>
    /// Implementasi Branch Service dengan role-based filtering
    /// </summary>
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<BranchService> _logger;

        public BranchService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<BranchService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<BranchDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check access permission
                if (!_currentUserService.CanAccessAllData() && !_currentUserService.CanAccessBranchData(id))
                {
                    return ApiResponse<BranchDto>.Forbidden("Anda tidak memiliki akses ke data branch ini");
                }

                var repository = _unitOfWork.Repository<Branch, int>();  // <-- PERBAIKAN DI SINI
                Branch? branch;

                // SuperAdmin bisa lihat deleted, yang lain tidak
                if (_currentUserService.IsSuperAdmin())
                {
                    branch = await repository.GetByIdIgnoreFiltersAsync(id, cancellationToken);
                }
                else
                {
                    branch = await repository.GetByIdAsync(id, cancellationToken);
                }

                if (branch == null)
                {
                    return ApiResponse<BranchDto>.NotFound("Branch");
                }

                var dto = await MapToDtoAsync(branch, cancellationToken);
                return ApiResponse<BranchDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch by id: {Id}", id);
                return ApiResponse<BranchDto>.Danger("Gagal mengambil data branch", ex);
            }
        }

        public async Task<ApiResponse<BranchDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<Branch, int>();  // <-- PERBAIKAN DI SINI
                var query = repository.Query();

                // BranchAdmin hanya bisa lihat branch sendiri
                if (_currentUserService.IsBranchAdmin() && !_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    query = query.Where(b => b.Id == _currentUserService.BranchId);
                }

                var branch = await query.FirstOrDefaultAsync(b => b.Code == code, cancellationToken);

                if (branch == null)
                {
                    return ApiResponse<BranchDto>.NotFound("Branch");
                }

                // Check access permission
                if (!_currentUserService.CanAccessAllData() && !_currentUserService.CanAccessBranchData(branch.Id))
                {
                    return ApiResponse<BranchDto>.Forbidden("Anda tidak memiliki akses ke data branch ini");
                }

                var dto = await MapToDtoAsync(branch, cancellationToken);
                return ApiResponse<BranchDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch by code: {Code}", code);
                return ApiResponse<BranchDto>.Danger("Gagal mengambil data branch", ex);
            }
        }

        public async Task<ApiResponse<List<BranchDto>>> GetAllAsync(BranchFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = BuildFilteredQuery(filter);
                var branches = await query.ToListAsync(cancellationToken);
                var dtos = await MapToDtosAsync(branches, cancellationToken);

                return ApiResponse<List<BranchDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all branches");
                return ApiResponse<List<BranchDto>>.Danger("Gagal mengambil data branch", ex);
            }
        }

        public async Task<ApiResponse<PaginatedList<BranchDto>>> GetPaginatedAsync(
            BranchFilterDto? filter = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = BuildFilteredQuery(filter);
                var totalCount = await query.CountAsync(cancellationToken);

                var branches = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var dtos = await MapToDtosAsync(branches, cancellationToken);

                var result = new PaginatedList<BranchDto>
                {
                    Items = dtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasNext = page * pageSize < totalCount,
                    HasPrevious = page > 1
                };

                return ApiResponse<PaginatedList<BranchDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated branches");
                return ApiResponse<PaginatedList<BranchDto>>.Danger("Gagal mengambil data branch", ex);
            }
        }

        public async Task<ApiResponse<BranchDto>> CreateAsync(CreateBranchDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya Admin dan SuperAdmin yang bisa create
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<BranchDto>.Forbidden("Hanya Admin yang dapat membuat branch");
                }

                // Check duplicate code
                if (!string.IsNullOrEmpty(dto.Code))
                {
                    var existing = await _unitOfWork.Repository<Branch, int>()  // <-- PERBAIKAN DI SINI
                        .QueryIgnoreFilters()
                        .AnyAsync(b => b.Code == dto.Code, cancellationToken);

                    if (existing)
                    {
                        return ApiResponse<BranchDto>.Error("Kode branch sudah digunakan", "Code already exists", 409);
                    }
                }

                var branch = _mapper.Map<Branch>(dto);

                await _unitOfWork.Repository<Branch, int>().AddAsync(branch, cancellationToken);  // <-- PERBAIKAN DI SINI
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var result = await MapToDtoAsync(branch, cancellationToken);
                return ApiResponse<BranchDto>.Created(result, "Branch berhasil dibuat");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating branch");
                return ApiResponse<BranchDto>.Danger("Gagal membuat branch", ex);
            }
        }

        public async Task<ApiResponse<BranchDto>> UpdateAsync(int id, UpdateBranchDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya Admin dan SuperAdmin yang bisa update
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<BranchDto>.Forbidden("Hanya Admin yang dapat mengupdate branch");
                }

                var repository = _unitOfWork.Repository<Branch, int>();  // <-- PERBAIKAN DI SINI
                var branch = await repository.GetByIdIgnoreFiltersAsync(id, cancellationToken);

                if (branch == null)
                {
                    return ApiResponse<BranchDto>.NotFound("Branch");
                }

                _mapper.Map(dto, branch);
                repository.Update(branch);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var result = await MapToDtoAsync(branch, cancellationToken);
                return ApiResponse<BranchDto>.Success(result, "Branch berhasil diupdate");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict updating branch: {Id}", id);
                return ApiResponse<BranchDto>.Error("Data sudah diubah oleh user lain", "Concurrency conflict", 409);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch: {Id}", id);
                return ApiResponse<BranchDto>.Danger("Gagal mengupdate branch", ex);
            }
        }

        public async Task<ApiResponse<bool>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya Admin dan SuperAdmin yang bisa delete
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat menghapus branch");
                }

                var repository = _unitOfWork.Repository<Branch, int>();  // <-- PERBAIKAN DI SINI
                var branch = await repository.GetByIdAsync(id, cancellationToken);

                if (branch == null)
                {
                    return ApiResponse<bool>.NotFound("Branch");
                }

                // Set BranchId user menjadi null sebelum delete
                await UnassignUsersFromBranchAsync(id, cancellationToken);

                repository.SoftDelete(branch, _currentUserService.UserId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Branch berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting branch: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal menghapus branch", ex);
            }
        }

        public async Task<ApiResponse<bool>> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya SuperAdmin yang bisa hard delete
                if (!_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<bool>.Forbidden("Hanya SuperAdmin yang dapat menghapus permanen");
                }

                var repository = _unitOfWork.Repository<Branch, int>();  // <-- PERBAIKAN DI SINI

                // Unassign users first
                await UnassignUsersFromBranchAsync(id, cancellationToken);

                var success = await repository.HardDeleteByIdAsync(id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (!success)
                {
                    return ApiResponse<bool>.NotFound("Branch");
                }

                return ApiResponse<bool>.Success(true, "Branch berhasil dihapus permanen");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting branch: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal menghapus branch", ex);
            }
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya SuperAdmin yang bisa restore
                if (!_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<bool>.Forbidden("Hanya SuperAdmin yang dapat restore");
                }

                var repository = _unitOfWork.Repository<Branch, int>();  // <-- PERBAIKAN DI SINI
                var branch = await repository.GetByIdIgnoreFiltersAsync(id, cancellationToken);

                if (branch == null)
                {
                    return ApiResponse<bool>.NotFound("Branch");
                }

                if (!branch.IsDeleted)
                {
                    return ApiResponse<bool>.Error("Branch tidak dalam status terhapus", "Not deleted", 400);
                }

                repository.Restore(branch, _currentUserService.UserId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Branch berhasil direstore");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring branch: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal restore branch", ex);
            }
        }

        public async Task<ApiResponse<bool>> ActivateAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat mengaktifkan branch");
                }

                var repository = _unitOfWork.Repository<Branch, int>();  // <-- PERBAIKAN DI SINI
                var branch = await repository.GetByIdAsync(id, cancellationToken);

                if (branch == null)
                {
                    return ApiResponse<bool>.NotFound("Branch");
                }

                branch.Activate(_currentUserService.UserId!);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Branch berhasil diaktifkan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating branch: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal mengaktifkan branch", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat menonaktifkan branch");
                }

                var repository = _unitOfWork.Repository<Branch, int>();  // <-- PERBAIKAN DI SINI
                var branch = await repository.GetByIdAsync(id, cancellationToken);

                if (branch == null)
                {
                    return ApiResponse<bool>.NotFound("Branch");
                }

                branch.Deactivate(_currentUserService.UserId!);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Branch berhasil dinonaktifkan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating branch: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal menonaktifkan branch", ex);
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Repository<Branch, int>().ExistsAsync(id, cancellationToken);  // <-- PERBAIKAN DI SINI
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<bool>> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Repository<Branch, int>()  // <-- PERBAIKAN DI SINI
                .QueryIgnoreFilters()
                .AnyAsync(b => b.Code == code, cancellationToken);
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<int>> CountAsync(BranchFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            var query = BuildFilteredQuery(filter);
            var count = await query.CountAsync(cancellationToken);
            return ApiResponse<int>.Success(count);
        }

        #region Private Methods

        private IQueryable<Branch> BuildFilteredQuery(BranchFilterDto? filter)
        {
            var repository = _unitOfWork.Repository<Branch, int>();  // <-- PERBAIKAN DI SINI
            IQueryable<Branch> query;

            // Role-based query source
            if (_currentUserService.IsSuperAdmin())
            {
                query = repository.QueryIgnoreFilters();
            }
            else
            {
                query = repository.Query();
            }

            // BranchAdmin hanya bisa lihat branch sendiri
            if (_currentUserService.IsBranchAdmin() && !_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
            {
                query = query.Where(b => b.Id == _currentUserService.BranchId);
            }

            // Apply filters
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(b =>
                        b.Name.ToLower().Contains(search) ||
                        (b.Code != null && b.Code.ToLower().Contains(search)) ||
                        (b.Description != null && b.Description.ToLower().Contains(search)) ||
                        (b.City != null && b.City.ToLower().Contains(search)));
                }

                if (!string.IsNullOrWhiteSpace(filter.City))
                {
                    query = query.Where(b => b.City == filter.City);
                }

                if (!string.IsNullOrWhiteSpace(filter.Province))
                {
                    query = query.Where(b => b.Province == filter.Province);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(b => b.IsActive == filter.IsActive.Value);
                }

                if (filter.IsMainBranch.HasValue)
                {
                    query = query.Where(b => b.IsMainBranch == filter.IsMainBranch.Value);
                }

                // Apply sorting
                if (!string.IsNullOrWhiteSpace(filter.SortBy))
                {
                    query = filter.SortBy.ToLower() switch
                    {
                        "name" => filter.SortDescending ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
                        "code" => filter.SortDescending ? query.OrderByDescending(b => b.Code) : query.OrderBy(b => b.Code),
                        "city" => filter.SortDescending ? query.OrderByDescending(b => b.City) : query.OrderBy(b => b.City),
                        "isactive" => filter.SortDescending ? query.OrderByDescending(b => b.IsActive) : query.OrderBy(b => b.IsActive),
                        _ => filter.SortDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt)
                    };
                }
                else
                {
                    query = query.OrderByDescending(b => b.CreatedAt);
                }
            }
            else
            {
                query = query.OrderByDescending(b => b.CreatedAt);
            }

            return query;
        }

        private async Task<BranchDto> MapToDtoAsync(Branch branch, CancellationToken cancellationToken)
        {
            var dto = _mapper.Map<BranchDto>(branch);

            // Hitung jumlah user
            dto.UserCount = await _unitOfWork.Repository<ApplicationUser, string>()  // <-- PERBAIKAN DI SINI
                .QueryIgnoreFilters()
                .CountAsync(u => u.BranchId == branch.Id, cancellationToken);

            return dto;
        }

        private async Task<List<BranchDto>> MapToDtosAsync(List<Branch> branches, CancellationToken cancellationToken)
        {
            var dtos = new List<BranchDto>();

            foreach (var branch in branches)
            {
                dtos.Add(await MapToDtoAsync(branch, cancellationToken));
            }

            return dtos;
        }

        private async Task UnassignUsersFromBranchAsync(int branchId, CancellationToken cancellationToken)
        {
            var userRepository = _unitOfWork.Repository<ApplicationUser, string>();  // <-- PERBAIKAN DI SINI
            var users = await userRepository
                .QueryIgnoreFilters()
                .Where(u => u.BranchId == branchId)
                .ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                user.BranchId = null;
                userRepository.Update(user);
            }
        }

        #endregion
    }

    public interface IHasRowVersion
    {
        byte[]? RowVersion { get; }
    }
}