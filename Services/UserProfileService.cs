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
    /// Implementasi UserProfile Service dengan role-based filtering
    /// </summary>
    public class UserProfileService : IUserProfileService
    {
        private readonly Interfaces.IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(
            Interfaces.IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<UserProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<UserProfileDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<UserProfile, int>();  // <-- PERBAIKAN DI SINI
                var profile = await repository.Query()
                    .Include(p => p.User)
                    .ThenInclude(u => u!.Branch)
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

                if (profile == null)
                {
                    return ApiResponse<UserProfileDto>.NotFound("User Profile");
                }

                // Check access permission
                if (!CanAccessProfile(profile))
                {
                    return ApiResponse<UserProfileDto>.Forbidden("Anda tidak memiliki akses ke profile ini");
                }

                var dto = MapToDto(profile);
                return ApiResponse<UserProfileDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile by id: {Id}", id);
                return ApiResponse<UserProfileDto>.Danger("Gagal mengambil data profile", ex);
            }
        }

        public async Task<ApiResponse<UserProfileDto>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check access permission
                if (!_currentUserService.CanAccessOwnData(userId))
                {
                    return ApiResponse<UserProfileDto>.Forbidden("Anda tidak memiliki akses ke profile ini");
                }

                var repository = _unitOfWork.Repository<UserProfile, int>();  // <-- PERBAIKAN DI SINI
                var profile = await repository.Query()
                    .Include(p => p.User)
                    .ThenInclude(u => u!.Branch)
                    .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

                if (profile == null)
                {
                    return ApiResponse<UserProfileDto>.NotFound("User Profile");
                }

                var dto = MapToDto(profile);
                return ApiResponse<UserProfileDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile by user id: {UserId}", userId);
                return ApiResponse<UserProfileDto>.Danger("Gagal mengambil data profile", ex);
            }
        }

        public async Task<ApiResponse<List<UserProfileDto>>> GetAllAsync(UserProfileFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // User biasa tidak boleh lihat list semua profile
                if (!_currentUserService.IsAuthenticated ||
                    (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin() && !_currentUserService.IsBranchAdmin()))
                {
                    return ApiResponse<List<UserProfileDto>>.Forbidden("Anda tidak memiliki akses ke daftar profile");
                }

                var query = BuildFilteredQuery(filter);
                var profiles = await query.ToListAsync(cancellationToken);
                var dtos = profiles.Select(MapToDto).ToList();

                return ApiResponse<List<UserProfileDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all user profiles");
                return ApiResponse<List<UserProfileDto>>.Danger("Gagal mengambil data profile", ex);
            }
        }

        public async Task<ApiResponse<PaginatedList<UserProfileDto>>> GetPaginatedAsync(
            UserProfileFilterDto? filter = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // User biasa tidak boleh lihat list semua profile
                if (!_currentUserService.IsAuthenticated ||
                    (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin() && !_currentUserService.IsBranchAdmin()))
                {
                    return ApiResponse<PaginatedList<UserProfileDto>>.Forbidden("Anda tidak memiliki akses ke daftar profile");
                }

                var query = BuildFilteredQuery(filter);
                var totalCount = await query.CountAsync(cancellationToken);

                var profiles = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var dtos = profiles.Select(MapToDto).ToList();

                var result = new PaginatedList<UserProfileDto>
                {
                    Items = dtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasNext = page * pageSize < totalCount,
                    HasPrevious = page > 1
                };

                return ApiResponse<PaginatedList<UserProfileDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated user profiles");
                return ApiResponse<PaginatedList<UserProfileDto>>.Danger("Gagal mengambil data profile", ex);
            }
        }

        public async Task<ApiResponse<UserProfileDto>> CreateAsync(CreateUserProfileDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if user exists
                var userRepository = _unitOfWork.Repository<ApplicationUser, string>();  // <-- SUDAH BENAR
                var user = await userRepository.QueryIgnoreFilters()
                    .FirstOrDefaultAsync(u => u.Id == dto.UserId, cancellationToken);

                if (user == null)
                {
                    return ApiResponse<UserProfileDto>.NotFound("User");
                }

                // Check permission - hanya admin atau user sendiri
                if (!_currentUserService.CanAccessOwnData(dto.UserId))
                {
                    return ApiResponse<UserProfileDto>.Forbidden("Anda tidak memiliki akses untuk membuat profile ini");
                }

                // Check if profile already exists
                var existingProfile = await _unitOfWork.Repository<UserProfile, int>()  // <-- PERBAIKAN DI SINI
                    .QueryIgnoreFilters()
                    .FirstOrDefaultAsync(p => p.UserId == dto.UserId, cancellationToken);

                if (existingProfile != null)
                {
                    return ApiResponse<UserProfileDto>.Error("Profile untuk user ini sudah ada", "Duplicate", 409);
                }

                var profile = _mapper.Map<UserProfile>(dto);
                profile.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Repository<UserProfile, int>().AddAsync(profile, cancellationToken);  // <-- PERBAIKAN DI SINI
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload dengan includes
                profile = await _unitOfWork.Repository<UserProfile, int>()  // <-- PERBAIKAN DI SINI
                    .Query()
                    .Include(p => p.User)
                    .ThenInclude(u => u!.Branch)
                    .FirstOrDefaultAsync(p => p.Id == profile.Id, cancellationToken);

                var result = MapToDto(profile!);
                return ApiResponse<UserProfileDto>.Created(result, "Profile berhasil dibuat");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user profile");
                return ApiResponse<UserProfileDto>.Danger("Gagal membuat profile", ex);
            }
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateAsync(int id, UpdateUserProfileDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<UserProfile, int>();  // <-- PERBAIKAN DI SINI
                var profile = await repository.Query()
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

                if (profile == null)
                {
                    return ApiResponse<UserProfileDto>.NotFound("User Profile");
                }

                // Check permission
                if (!CanAccessProfile(profile))
                {
                    return ApiResponse<UserProfileDto>.Forbidden("Anda tidak memiliki akses untuk mengupdate profile ini");
                }

                _mapper.Map(dto, profile);
                profile.ModifiedAt = DateTime.UtcNow;

                repository.Update(profile);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload dengan includes
                profile = await repository.Query()
                    .Include(p => p.User)
                    .ThenInclude(u => u!.Branch)
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

                var result = MapToDto(profile!);
                return ApiResponse<UserProfileDto>.Success(result, "Profile berhasil diupdate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile: {Id}", id);
                return ApiResponse<UserProfileDto>.Danger("Gagal mengupdate profile", ex);
            }
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateByUserIdAsync(string userId, UpdateUserProfileDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check permission
                if (!_currentUserService.CanAccessOwnData(userId))
                {
                    return ApiResponse<UserProfileDto>.Forbidden("Anda tidak memiliki akses untuk mengupdate profile ini");
                }

                var repository = _unitOfWork.Repository<UserProfile, int>();  // <-- PERBAIKAN DI SINI
                var profile = await repository.Query()
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

                if (profile == null)
                {
                    return ApiResponse<UserProfileDto>.NotFound("User Profile");
                }

                _mapper.Map(dto, profile);
                profile.ModifiedAt = DateTime.UtcNow;

                repository.Update(profile);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload dengan includes
                profile = await repository.Query()
                    .Include(p => p.User)
                    .ThenInclude(u => u!.Branch)
                    .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

                var result = MapToDto(profile!);
                return ApiResponse<UserProfileDto>.Success(result, "Profile berhasil diupdate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile by user id: {UserId}", userId);
                return ApiResponse<UserProfileDto>.Danger("Gagal mengupdate profile", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<UserProfile, int>();  // <-- PERBAIKAN DI SINI
                var profile = await repository.Query()
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

                if (profile == null)
                {
                    return ApiResponse<bool>.NotFound("User Profile");
                }

                // Check permission - hanya admin atau user sendiri
                if (!_currentUserService.CanAccessOwnData(profile.UserId))
                {
                    return ApiResponse<bool>.Forbidden("Anda tidak memiliki akses untuk menghapus profile ini");
                }

                repository.HardDelete(profile);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Profile berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user profile: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal menghapus profile", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeleteByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check permission
                if (!_currentUserService.CanAccessOwnData(userId))
                {
                    return ApiResponse<bool>.Forbidden("Anda tidak memiliki akses untuk menghapus profile ini");
                }

                var repository = _unitOfWork.Repository<UserProfile, int>();  // <-- PERBAIKAN DI SINI
                var profile = await repository.Query()
                    .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

                if (profile == null)
                {
                    return ApiResponse<bool>.NotFound("User Profile");
                }

                repository.HardDelete(profile);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Profile berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user profile by user id: {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal menghapus profile", ex);
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Repository<UserProfile, int>().ExistsAsync(id, cancellationToken);  // <-- PERBAIKAN DI SINI
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<bool>> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Repository<UserProfile, int>()  // <-- PERBAIKAN DI SINI
                .QueryIgnoreFilters()
                .AnyAsync(p => p.UserId == userId, cancellationToken);
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<int>> CountAsync(UserProfileFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            var query = BuildFilteredQuery(filter);
            var count = await query.CountAsync(cancellationToken);
            return ApiResponse<int>.Success(count);
        }

        #region Private Methods

        private IQueryable<UserProfile> BuildFilteredQuery(UserProfileFilterDto? filter)
        {
            var repository = _unitOfWork.Repository<UserProfile, int>();  // <-- PERBAIKAN DI SINI
            var query = repository.Query()
                .Include(p => p.User)
                .ThenInclude(u => u!.Branch)
                .AsQueryable();

            // Role-based filtering
            if (_currentUserService.IsBranchAdmin() && !_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
            {
                // BranchAdmin hanya lihat profile user di branch yang sama
                query = query.Where(p => p.User!.BranchId == _currentUserService.BranchId);
            }

            // Apply filters
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(p =>
                        p.User!.UserName!.ToLower().Contains(search) ||
                        p.User.Email!.ToLower().Contains(search) ||
                        (p.User.FullName != null && p.User.FullName.ToLower().Contains(search)) ||
                        (p.About != null && p.About.ToLower().Contains(search)));
                }

                if (!string.IsNullOrWhiteSpace(filter.Gender))
                {
                    query = query.Where(p => p.Gender == filter.Gender);
                }

                if (!string.IsNullOrWhiteSpace(filter.City))
                {
                    query = query.Where(p => p.User!.Branch!.City == filter.City);
                }

                if (filter.BranchId.HasValue)
                {
                    query = query.Where(p => p.User!.BranchId == filter.BranchId.Value);
                }

                if (filter.NewsletterSubscribed.HasValue)
                {
                    query = query.Where(p => p.NewsletterSubscribed == filter.NewsletterSubscribed.Value);
                }

                // Apply sorting
                if (!string.IsNullOrWhiteSpace(filter.SortBy))
                {
                    query = filter.SortBy.ToLower() switch
                    {
                        "username" => filter.SortDescending ? query.OrderByDescending(p => p.User!.UserName) : query.OrderBy(p => p.User!.UserName),
                        "fullname" => filter.SortDescending ? query.OrderByDescending(p => p.User!.FullName) : query.OrderBy(p => p.User!.FullName),
                        "balance" => filter.SortDescending ? query.OrderByDescending(p => p.Balance) : query.OrderBy(p => p.Balance),
                        _ => filter.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
                    };
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedAt);
                }
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            return query;
        }

        private bool CanAccessProfile(UserProfile profile)
        {
            // SuperAdmin dan Admin bisa akses semua
            if (_currentUserService.CanAccessAllData())
                return true;

            // BranchAdmin bisa akses profile user di branch yang sama
            if (_currentUserService.IsBranchAdmin() && profile.User?.BranchId == _currentUserService.BranchId)
                return true;

            // User bisa akses profile sendiri
            if (_currentUserService.UserId == profile.UserId)
                return true;

            return false;
        }

        private UserProfileDto MapToDto(UserProfile profile)
        {
            var dto = _mapper.Map<UserProfileDto>(profile);

            if (profile.User != null)
            {
                dto.UserName = profile.User.UserName;
                dto.UserEmail = profile.User.Email;
                dto.FullName = profile.User.FullName;
                dto.BranchId = profile.User.BranchId;
                dto.BranchName = profile.User.Branch?.Name;
            }

            return dto;
        }

        #endregion
    }
}
