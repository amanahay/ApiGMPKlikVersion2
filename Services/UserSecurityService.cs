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
    /// Implementasi UserSecurity Service dengan role-based filtering
    /// </summary>
    public class UserSecurityService : IUserSecurityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserSecurityService> _logger;

        public UserSecurityService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<UserSecurityService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<UserSecurityDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<UserSecurity, int>();
                var security = await repository.Query()
                    .Include(s => s.User)
                    .ThenInclude(u => u!.Branch)
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken: cancellationToken);

                if (security == null)
                {
                    return ApiResponse<UserSecurityDto>.NotFound("User Security");
                }

                // Check access permission
                if (!CanAccessSecurity(security))
                {
                    return ApiResponse<UserSecurityDto>.Forbidden("Anda tidak memiliki akses ke data security ini");
                }

                var dto = MapToDto(security);
                return ApiResponse<UserSecurityDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user security by id: {Id}", id);
                return ApiResponse<UserSecurityDto>.Danger("Gagal mengambil data security", ex);
            }
        }

        public async Task<ApiResponse<UserSecurityDto>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check access permission
                if (!_currentUserService.CanAccessOwnData(userId))
                {
                    return ApiResponse<UserSecurityDto>.Forbidden("Anda tidak memiliki akses ke data security ini");
                }

                var repository = _unitOfWork.Repository<UserSecurity, int>();
                var security = await repository.Query()
                    .Include(s => s.User)
                    .ThenInclude(u => u!.Branch)
                    .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken: cancellationToken);

                if (security == null)
                {
                    return ApiResponse<UserSecurityDto>.NotFound("User Security");
                }

                var dto = MapToDto(security);
                return ApiResponse<UserSecurityDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user security by user id: {UserId}", userId);
                return ApiResponse<UserSecurityDto>.Danger("Gagal mengambil data security", ex);
            }
        }

        public async Task<ApiResponse<List<UserSecurityDto>>> GetAllAsync(UserSecurityFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya Admin dan SuperAdmin yang bisa lihat semua
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<List<UserSecurityDto>>.Forbidden("Hanya Admin yang dapat melihat daftar security");
                }

                var query = BuildFilteredQuery(filter);
                var securities = await query.ToListAsync(cancellationToken);
                var dtos = securities.Select(MapToDto).ToList();

                return ApiResponse<List<UserSecurityDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all user securities");
                return ApiResponse<List<UserSecurityDto>>.Danger("Gagal mengambil data security", ex);
            }
        }

        public async Task<ApiResponse<PaginatedList<UserSecurityDto>>> GetPaginatedAsync(
            UserSecurityFilterDto? filter = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya Admin dan SuperAdmin yang bisa lihat semua
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<PaginatedList<UserSecurityDto>>.Forbidden("Hanya Admin yang dapat melihat daftar security");
                }

                var query = BuildFilteredQuery(filter);
                var totalCount = await query.CountAsync(cancellationToken);

                var securities = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var dtos = securities.Select(MapToDto).ToList();

                var result = new PaginatedList<UserSecurityDto>
                {
                    Items = dtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasNext = page * pageSize < totalCount,
                    HasPrevious = page > 1
                };

                return ApiResponse<PaginatedList<UserSecurityDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated user securities");
                return ApiResponse<PaginatedList<UserSecurityDto>>.Danger("Gagal mengambil data security", ex);
            }
        }

        public async Task<ApiResponse<UserSecurityDto>> CreateAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if user exists
                var userRepository = _unitOfWork.Repository<ApplicationUser, string>();
                var user = await userRepository.QueryIgnoreFilters()
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

                if (user == null)
                {
                    return ApiResponse<UserSecurityDto>.NotFound("User");
                }

                // Check permission
                if (!_currentUserService.CanAccessOwnData(userId))
                {
                    return ApiResponse<UserSecurityDto>.Forbidden("Anda tidak memiliki akses untuk membuat security ini");
                }

                // Check if security already exists
                var existingSecurity = await _unitOfWork.Repository<UserSecurity, int>()
                    .Query()
                    .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken: cancellationToken);

                if (existingSecurity != null)
                {
                    return ApiResponse<UserSecurityDto>.Error("Security untuk user ini sudah ada", "Duplicate", 409);
                }

                var security = new UserSecurity
                {
                    UserId = userId,
                    FailedLoginAttempts = 0
                };

                await _unitOfWork.Repository<UserSecurity, int>().AddAsync(security);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload dengan includes
                security = await _unitOfWork.Repository<UserSecurity, int>().Query()
                    .Include(s => s.User)
                    .ThenInclude(u => u!.Branch)
                    .FirstOrDefaultAsync(s => s.Id == security.Id, cancellationToken: cancellationToken);

                var result = MapToDto(security!);
                return ApiResponse<UserSecurityDto>.Created(result, "Security berhasil dibuat");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user security");
                return ApiResponse<UserSecurityDto>.Danger("Gagal membuat security", ex);
            }
        }

        public async Task<ApiResponse<UserSecurityDto>> UpdateAsync(int id, UpdateUserSecurityDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<UserSecurity, int>();
                var security = await repository.Query()
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                if (security == null)
                {
                    return ApiResponse<UserSecurityDto>.NotFound("User Security");
                }

                // Check permission
                if (!CanAccessSecurity(security))
                {
                    return ApiResponse<UserSecurityDto>.Forbidden("Anda tidak memiliki akses untuk mengupdate security ini");
                }

                _mapper.Map(dto, security);
                repository.Update(security);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload dengan includes
                security = await repository.Query()
                    .Include(s => s.User)
                    .ThenInclude(u => u!.Branch)
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken: cancellationToken);

                var result = MapToDto(security!);
                return ApiResponse<UserSecurityDto>.Success(result, "Security berhasil diupdate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user security: {Id}", id);
                return ApiResponse<UserSecurityDto>.Danger("Gagal mengupdate security", ex);
            }
        }

        public async Task<ApiResponse<bool>> LockUserAsync(string userId, DateTime lockedUntil, string? reason = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya Admin dan SuperAdmin yang bisa lock
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat mengunci user");
                }

                var repository = _unitOfWork.Repository<UserSecurity, int>();
                var security = await repository.Query()
                    .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken: cancellationToken);

                if (security == null)
                {
                    return ApiResponse<bool>.NotFound("User Security");
                }

                security.LockedUntil = lockedUntil;
                repository.Update(security);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} locked until {LockedUntil}. Reason: {Reason}",
                    userId, lockedUntil, reason ?? "Not specified");

                return ApiResponse<bool>.Success(true, $"User berhasil dikunci sampai {lockedUntil:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user: {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal mengunci user", ex);
            }
        }

        public async Task<ApiResponse<bool>> UnlockUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya Admin dan SuperAdmin yang bisa unlock
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat membuka kunci user");
                }

                var repository = _unitOfWork.Repository<UserSecurity, int>();
                var security = await repository.Query()
                    .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken: cancellationToken);

                if (security == null)
                {
                    return ApiResponse<bool>.NotFound("User Security");
                }

                security.LockedUntil = null;
                security.FailedLoginAttempts = 0;
                repository.Update(security);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} unlocked by {UnlockedBy}",
                    userId, _currentUserService.UserId);

                return ApiResponse<bool>.Success(true, "User berhasil dibuka kuncinya");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user: {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal membuka kunci user", ex);
            }
        }

        public async Task<ApiResponse<bool>> ResetFailedAttemptsAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Hanya Admin dan SuperAdmin yang bisa reset
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat reset failed attempts");
                }

                var repository = _unitOfWork.Repository<UserSecurity, int>();
                var security = await repository.Query()
                    .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken: cancellationToken);

                if (security == null)
                {
                    return ApiResponse<bool>.NotFound("User Security");
                }

                var oldAttempts = security.FailedLoginAttempts;
                security.FailedLoginAttempts = 0;
                security.LastFailedLoginAt = null;
                repository.Update(security);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Failed attempts for user {UserId} reset from {OldAttempts} to 0 by {ResetBy}",
                    userId, oldAttempts, _currentUserService.UserId);

                return ApiResponse<bool>.Success(true, "Failed attempts berhasil direset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting failed attempts for user: {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal reset failed attempts", ex);
            }
        }

        public async Task<ApiResponse<bool>> RecordLoginSuccessAsync(string userId, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<UserSecurity, int>();
                var security = await repository.Query()
                    .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken: cancellationToken);

                if (security == null)
                {
                    // Create security record if not exists
                    security = new UserSecurity
                    {
                        UserId = userId,
                        FailedLoginAttempts = 0
                    };
                    await repository.AddAsync(security);
                }

                security.LastLoginAt = DateTime.UtcNow;
                security.LastLoginIp = ipAddress;
                security.FailedLoginAttempts = 0;
                security.LastFailedLoginAt = null;

                repository.Update(security);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording login success for user: {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal merekam login success", ex);
            }
        }

        public async Task<ApiResponse<bool>> RecordLoginFailureAsync(string userId, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<UserSecurity, int>();
                var security = await repository.Query()
                    .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken: cancellationToken);

                if (security == null)
                {
                    // Create security record if not exists
                    security = new UserSecurity
                    {
                        UserId = userId,
                        FailedLoginAttempts = 0
                    };
                    await repository.AddAsync(security);
                }

                security.FailedLoginAttempts++;
                security.LastFailedLoginAt = DateTime.UtcNow;

                repository.Update(security);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Login failure recorded for user {UserId}. Attempt #{Attempts} from IP {Ip}",
                    userId, security.FailedLoginAttempts, ipAddress ?? "Unknown");

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording login failure for user: {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal merekam login failure", ex);
            }
        }

        public async Task<ApiResponse<bool>> IsUserLockedAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<UserSecurity, int>();
                var security = await repository.Query()
                    .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken: cancellationToken);

                if (security == null)
                {
                    return ApiResponse<bool>.Success(false);
                }

                var isLocked = security.LockedUntil.HasValue && security.LockedUntil > DateTime.UtcNow;
                return ApiResponse<bool>.Success(isLocked);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is locked: {UserId}", userId);
                return ApiResponse<bool>.Danger("Gagal mengecek status lock", ex);
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Repository<UserSecurity, int>().ExistsAsync(id, cancellationToken);
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<bool>> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Repository<UserSecurity, int>()
                .Query()
                .AnyAsync(s => s.UserId == userId, cancellationToken);
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<int>> CountAsync(UserSecurityFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            var query = BuildFilteredQuery(filter);
            var count = await query.CountAsync(cancellationToken);
            return ApiResponse<int>.Success(count);
        }

        #region Private Methods

        private IQueryable<UserSecurity> BuildFilteredQuery(UserSecurityFilterDto? filter)
        {
            var repository = _unitOfWork.Repository<UserSecurity, int>();
            var query = repository.Query()
                .Include(s => s.User)
                .ThenInclude(u => u!.Branch)
                .AsQueryable();

            // Role-based filtering
            if (_currentUserService.IsBranchAdmin() && !_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
            {
                // BranchAdmin hanya lihat security user di branch yang sama
                query = query.Where(s => s.User!.BranchId == _currentUserService.BranchId);
            }

            // Apply filters
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(s =>
                        s.User!.UserName!.ToLower().Contains(search) ||
                        s.User.Email!.ToLower().Contains(search) ||
                        (s.User.FullName != null && s.User.FullName.ToLower().Contains(search)));
                }

                if (filter.IsLocked.HasValue)
                {
                    if (filter.IsLocked.Value)
                    {
                        query = query.Where(s => s.LockedUntil.HasValue && s.LockedUntil > DateTime.UtcNow);
                    }
                    else
                    {
                        query = query.Where(s => !s.LockedUntil.HasValue || s.LockedUntil <= DateTime.UtcNow);
                    }
                }

                if (filter.MinFailedAttempts.HasValue)
                {
                    query = query.Where(s => s.FailedLoginAttempts >= filter.MinFailedAttempts.Value);
                }

                if (filter.BranchId.HasValue)
                {
                    query = query.Where(s => s.User!.BranchId == filter.BranchId.Value);
                }

                // Apply sorting
                if (!string.IsNullOrWhiteSpace(filter.SortBy))
                {
                    query = filter.SortBy.ToLower() switch
                    {
                        "username" => filter.SortDescending ? query.OrderByDescending(s => s.User!.UserName) : query.OrderBy(s => s.User!.UserName),
                        "failedattempts" => filter.SortDescending ? query.OrderByDescending(s => s.FailedLoginAttempts) : query.OrderBy(s => s.FailedLoginAttempts),
                        "lastlogin" => filter.SortDescending ? query.OrderByDescending(s => s.LastLoginAt) : query.OrderBy(s => s.LastLoginAt),
                        _ => filter.SortDescending ? query.OrderByDescending(s => s.LastLoginAt) : query.OrderBy(s => s.LastLoginAt)
                    };
                }
                else
                {
                    query = query.OrderByDescending(s => s.LastLoginAt);
                }
            }
            else
            {
                query = query.OrderByDescending(s => s.LastLoginAt);
            }

            return query;
        }

        private bool CanAccessSecurity(UserSecurity security)
        {
            // SuperAdmin dan Admin bisa akses semua
            if (_currentUserService.CanAccessAllData())
                return true;

            // BranchAdmin bisa akses security user di branch yang sama
            if (_currentUserService.IsBranchAdmin() && security.User?.BranchId == _currentUserService.BranchId)
                return true;

            // User bisa akses security sendiri
            if (_currentUserService.UserId == security.UserId)
                return true;

            return false;
        }

        private UserSecurityDto MapToDto(UserSecurity security)
        {
            var dto = _mapper.Map<UserSecurityDto>(security);

            if (security.User != null)
            {
                dto.UserName = security.User.UserName;
                dto.UserEmail = security.User.Email;
                dto.FullName = security.User.FullName;
                dto.BranchId = security.User.BranchId;
                dto.BranchName = security.User.Branch?.Name;
            }

            return dto;
        }

        #endregion
    }
}