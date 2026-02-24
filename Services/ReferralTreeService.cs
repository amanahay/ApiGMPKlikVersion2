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
    /// Implementasi ReferralTree Service dengan role-based filtering
    /// </summary>
    public class ReferralTreeService : IReferralTreeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<ReferralTreeService> _logger;

        public ReferralTreeService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<ReferralTreeService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<ReferralTreeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<ReferralTree, int>();
                var referral = await repository.Query()
                    .Include(r => r.RootUser)
                    .Include(r => r.ReferredUser)
                    .Include(r => r.ParentUser)
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (referral == null)
                {
                    return ApiResponse<ReferralTreeDto>.NotFound("Referral");
                }

                // Check access permission
                if (!CanAccessReferral(referral))
                {
                    return ApiResponse<ReferralTreeDto>.Forbidden("Anda tidak memiliki akses ke data referral ini");
                }

                var dto = MapToDto(referral);
                return ApiResponse<ReferralTreeDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting referral by id: {Id}", id);
                return ApiResponse<ReferralTreeDto>.Danger("Gagal mengambil data referral", ex);
            }
        }

        public async Task<ApiResponse<List<ReferralTreeDto>>> GetAllAsync(ReferralTreeFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = BuildFilteredQuery(filter);
                var referrals = await query.ToListAsync(cancellationToken);
                var dtos = referrals.Select(MapToDto).ToList();

                return ApiResponse<List<ReferralTreeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all referrals");
                return ApiResponse<List<ReferralTreeDto>>.Danger("Gagal mengambil data referral", ex);
            }
        }

        public async Task<ApiResponse<PaginatedList<ReferralTreeDto>>> GetPaginatedAsync(
            ReferralTreeFilterDto? filter = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = BuildFilteredQuery(filter);
                var totalCount = await query.CountAsync(cancellationToken);

                var referrals = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var dtos = referrals.Select(MapToDto).ToList();

                var result = new PaginatedList<ReferralTreeDto>
                {
                    Items = dtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasNext = page * pageSize < totalCount,
                    HasPrevious = page > 1
                };

                return ApiResponse<PaginatedList<ReferralTreeDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated referrals");
                return ApiResponse<PaginatedList<ReferralTreeDto>>.Danger("Gagal mengambil data referral", ex);
            }
        }

        public async Task<ApiResponse<List<ReferralTreeNodeDto>>> GetTreeByRootUserIdAsync(string rootUserId, int? maxLevel = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check access permission
                if (!_currentUserService.CanAccessOwnData(rootUserId))
                {
                    return ApiResponse<List<ReferralTreeNodeDto>>.Forbidden("Anda tidak memiliki akses ke tree ini");
                }

                var query = _unitOfWork.Repository<ReferralTree, int>().Query()
                    .Include(r => r.ReferredUser)
                    .Where(r => r.RootUserId == rootUserId && r.IsActive && !r.IsDeleted);

                if (maxLevel.HasValue)
                {
                    query = query.Where(r => r.Level <= maxLevel.Value);
                }

                var referrals = await query
                    .OrderBy(r => r.Level)
                    .ThenBy(r => r.CreatedAt)
                    .ToListAsync(cancellationToken);

                var dtos = referrals.Select(r => new ReferralTreeNodeDto
                {
                    UserId = r.ReferredUserId,
                    UserName = r.ReferredUser?.UserName,
                    Email = r.ReferredUser?.Email,
                    FullName = r.ReferredUser?.FullName,
                    Level = r.Level,
                    ParentUserId = r.ParentUserId,
                    CommissionPercent = r.CommissionPercent,
                    JoinedAt = r.CreatedAt,
                    IsActive = r.IsActive
                }).ToList();

                return ApiResponse<List<ReferralTreeNodeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tree by root user id: {RootUserId}", rootUserId);
                return ApiResponse<List<ReferralTreeNodeDto>>.Danger("Gagal mengambil data tree", ex);
            }
        }

        public async Task<ApiResponse<TreeNodeDto<ReferralTreeNodeData>>> GetTreeStructureAsync(string rootUserId, int? maxLevel = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check access permission
                if (!_currentUserService.CanAccessOwnData(rootUserId))
                {
                    return ApiResponse<TreeNodeDto<ReferralTreeNodeData>>.Forbidden("Anda tidak memiliki akses ke tree ini");
                }

                var query = _unitOfWork.Repository<ReferralTree, int>().Query()
                    .Include(r => r.ReferredUser)
                    .Where(r => r.RootUserId == rootUserId && r.IsActive && !r.IsDeleted);

                if (maxLevel.HasValue)
                {
                    query = query.Where(r => r.Level <= maxLevel.Value);
                }

                var referrals = await query.ToListAsync(cancellationToken);

                // Build tree structure
                var rootNode = BuildTreeStructure(rootUserId, referrals);

                if (rootNode == null)
                {
                    return ApiResponse<TreeNodeDto<ReferralTreeNodeData>>.NotFound("Tree");
                }

                return ApiResponse<TreeNodeDto<ReferralTreeNodeData>>.Success(rootNode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tree structure for root user id: {RootUserId}", rootUserId);
                return ApiResponse<TreeNodeDto<ReferralTreeNodeData>>.Danger("Gagal mengambil struktur tree", ex);
            }
        }

        public async Task<ApiResponse<List<ReferralTreeNodeDto>>> GetReferralsByUserIdAsync(string userId, int? level = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check access permission
                if (!_currentUserService.CanAccessOwnData(userId))
                {
                    return ApiResponse<List<ReferralTreeNodeDto>>.Forbidden("Anda tidak memiliki akses ke data ini");
                }

                var query = _unitOfWork.Repository<ReferralTree, int>().Query()
                    .Include(r => r.ReferredUser)
                    .Where(r => r.RootUserId == userId && r.IsActive && !r.IsDeleted);

                if (level.HasValue)
                {
                    query = query.Where(r => r.Level == level.Value);
                }

                var referrals = await query
                    .OrderBy(r => r.Level)
                    .ThenBy(r => r.CreatedAt)
                    .ToListAsync(cancellationToken);

                var dtos = referrals.Select(r => new ReferralTreeNodeDto
                {
                    UserId = r.ReferredUserId,
                    UserName = r.ReferredUser?.UserName,
                    Email = r.ReferredUser?.Email,
                    FullName = r.ReferredUser?.FullName,
                    Level = r.Level,
                    ParentUserId = r.ParentUserId,
                    CommissionPercent = r.CommissionPercent,
                    JoinedAt = r.CreatedAt,
                    IsActive = r.IsActive
                }).ToList();

                return ApiResponse<List<ReferralTreeNodeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting referrals by user id: {UserId}", userId);
                return ApiResponse<List<ReferralTreeNodeDto>>.Danger("Gagal mengambil data referral", ex);
            }
        }

        public async Task<ApiResponse<List<ReferralTreeNodeDto>>> GetAncestorsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check access permission
                if (!_currentUserService.CanAccessOwnData(userId))
                {
                    return ApiResponse<List<ReferralTreeNodeDto>>.Forbidden("Anda tidak memiliki akses ke data ini");
                }

                // Get all referrals where this user is the referred user
                var referrals = await _unitOfWork.Repository<ReferralTree, int>().Query()
                    .Include(r => r.RootUser)
                    .Include(r => r.ParentUser)
                    .Where(r => r.ReferredUserId == userId && r.IsActive && !r.IsDeleted)
                    .OrderBy(r => r.Level)
                    .ToListAsync(cancellationToken);

                var dtos = referrals.Select(r => new ReferralTreeNodeDto
                {
                    UserId = r.RootUserId,
                    UserName = r.RootUser?.UserName,
                    Email = r.RootUser?.Email,
                    FullName = r.RootUser?.FullName,
                    Level = r.Level,
                    ParentUserId = r.ParentUserId,
                    CommissionPercent = r.CommissionPercent,
                    JoinedAt = r.CreatedAt,
                    IsActive = r.IsActive
                }).ToList();

                return ApiResponse<List<ReferralTreeNodeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ancestors by user id: {UserId}", userId);
                return ApiResponse<List<ReferralTreeNodeDto>>.Danger("Gagal mengambil data ancestors", ex);
            }
        }

        public async Task<ApiResponse<ReferralTreeDto>> CreateAsync(CreateReferralTreeDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate level
                if (dto.Level < 1 || dto.Level > 3)
                {
                    return ApiResponse<ReferralTreeDto>.Error("Level harus antara 1-3", "Invalid level", 400);
                }

                // Check if users exist
                var userRepository = _unitOfWork.Repository<ApplicationUser, string>();
                var rootUser = await userRepository.QueryIgnoreFilters()
                    .FirstOrDefaultAsync(u => u.Id == dto.RootUserId, cancellationToken);

                if (rootUser == null)
                {
                    return ApiResponse<ReferralTreeDto>.NotFound("Root User");
                }

                var referredUser = await userRepository.QueryIgnoreFilters()
                    .FirstOrDefaultAsync(u => u.Id == dto.ReferredUserId, cancellationToken);

                if (referredUser == null)
                {
                    return ApiResponse<ReferralTreeDto>.NotFound("Referred User");
                }

                // Check permission
                if (!_currentUserService.CanAccessOwnData(dto.RootUserId) &&
                    !_currentUserService.IsAdmin() &&
                    !_currentUserService.IsSuperAdmin())
                {
                    return ApiResponse<ReferralTreeDto>.Forbidden("Anda tidak memiliki akses untuk membuat referral ini");
                }

                // Check if referral already exists
                var existing = await _unitOfWork.Repository<ReferralTree, int>().Query()
                    .FirstOrDefaultAsync(r => r.RootUserId == dto.RootUserId && r.ReferredUserId == dto.ReferredUserId, cancellationToken);

                if (existing != null)
                {
                    return ApiResponse<ReferralTreeDto>.Error("Referral sudah ada", "Duplicate", 409);
                }

                // Check if can add referral
                var canAdd = await CanAddReferralAsync(dto.RootUserId, dto.ReferredUserId, cancellationToken);
                if (!canAdd.Data)
                {
                    return ApiResponse<ReferralTreeDto>.Error("Tidak dapat menambahkan referral", "Invalid referral", 400);
                }

                var referral = _mapper.Map<ReferralTree>(dto);
                referral.IsActive = true;

                await _unitOfWork.Repository<ReferralTree, int>().AddAsync(referral, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload dengan includes
                referral = await _unitOfWork.Repository<ReferralTree, int>().Query()
                    .Include(r => r.RootUser)
                    .Include(r => r.ReferredUser)
                    .Include(r => r.ParentUser)
                    .FirstOrDefaultAsync(r => r.Id == referral.Id, cancellationToken);

                var result = MapToDto(referral!);
                return ApiResponse<ReferralTreeDto>.Created(result, "Referral berhasil dibuat");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating referral");
                return ApiResponse<ReferralTreeDto>.Danger("Gagal membuat referral", ex);
            }
        }

        public async Task<ApiResponse<ReferralTreeDto>> UpdateAsync(int id, UpdateReferralTreeDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<ReferralTree, int>();
                var referral = await repository.Query()
                    .Include(r => r.RootUser)
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (referral == null)
                {
                    return ApiResponse<ReferralTreeDto>.NotFound("Referral");
                }

                // Check permission
                if (!CanAccessReferral(referral))
                {
                    return ApiResponse<ReferralTreeDto>.Forbidden("Anda tidak memiliki akses untuk mengupdate referral ini");
                }

                _mapper.Map(dto, referral);
                repository.Update(referral);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload dengan includes
                referral = await repository.Query()
                    .Include(r => r.RootUser)
                    .Include(r => r.ReferredUser)
                    .Include(r => r.ParentUser)
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                var result = MapToDto(referral!);
                return ApiResponse<ReferralTreeDto>.Success(result, "Referral berhasil diupdate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating referral: {Id}", id);
                return ApiResponse<ReferralTreeDto>.Danger("Gagal mengupdate referral", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<ReferralTree, int>();
                var referral = await repository.Query()
                    .Include(r => r.RootUser)
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (referral == null)
                {
                    return ApiResponse<bool>.NotFound("Referral");
                }

                // Check permission
                if (!CanAccessReferral(referral))
                {
                    return ApiResponse<bool>.Forbidden("Anda tidak memiliki akses untuk menghapus referral ini");
                }

                // Hard delete untuk ReferralTree (atau bisa gunakan SoftDelete jika diinginkan)
                repository.HardDelete(referral);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Referral berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting referral: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal menghapus referral", ex);
            }
        }

        public async Task<ApiResponse<bool>> ActivateAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<ReferralTree, int>();
                var referral = await repository.Query()
                    .Include(r => r.RootUser)
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (referral == null)
                {
                    return ApiResponse<bool>.NotFound("Referral");
                }

                // Check permission
                if (!CanAccessReferral(referral))
                {
                    return ApiResponse<bool>.Forbidden("Anda tidak memiliki akses untuk mengaktifkan referral ini");
                }

                referral.IsActive = true;
                repository.Update(referral);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Referral berhasil diaktifkan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating referral: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal mengaktifkan referral", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var repository = _unitOfWork.Repository<ReferralTree, int>();
                var referral = await repository.Query()
                    .Include(r => r.RootUser)
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (referral == null)
                {
                    return ApiResponse<bool>.NotFound("Referral");
                }

                // Check permission
                if (!CanAccessReferral(referral))
                {
                    return ApiResponse<bool>.Forbidden("Anda tidak memiliki akses untuk menonaktifkan referral ini");
                }

                referral.IsActive = false;
                repository.Update(referral);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Referral berhasil dinonaktifkan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating referral: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal menonaktifkan referral", ex);
            }
        }

        public async Task<ApiResponse<ReferralStatisticsDto>> GetStatisticsAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check access permission
                if (!_currentUserService.CanAccessOwnData(userId))
                {
                    return ApiResponse<ReferralStatisticsDto>.Forbidden("Anda tidak memiliki akses ke statistik ini");
                }

                var query = _unitOfWork.Repository<ReferralTree, int>().Query()
                    .Where(r => r.RootUserId == userId && r.IsActive && !r.IsDeleted);

                var level1Count = await query.CountAsync(r => r.Level == 1, cancellationToken);
                var level2Count = await query.CountAsync(r => r.Level == 2, cancellationToken);
                var level3Count = await query.CountAsync(r => r.Level == 3, cancellationToken);
                var totalCommission = await query.SumAsync(r => r.CommissionPercent, cancellationToken);

                var user = await _unitOfWork.Repository<ApplicationUser, string>().QueryIgnoreFilters()
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                var statistics = new ReferralStatisticsDto
                {
                    UserId = userId,
                    UserName = user?.UserName,
                    TotalReferrals = level1Count + level2Count + level3Count,
                    Level1Count = level1Count,
                    Level2Count = level2Count,
                    Level3Count = level3Count,
                    TotalCommission = totalCommission,
                    LevelStats = new List<LevelStatistics>
                    {
                        new() { Level = 1, Count = level1Count, CommissionPercent = 0 },
                        new() { Level = 2, Count = level2Count, CommissionPercent = 0 },
                        new() { Level = 3, Count = level3Count, CommissionPercent = 0 }
                    }
                };

                return ApiResponse<ReferralStatisticsDto>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for user: {UserId}", userId);
                return ApiResponse<ReferralStatisticsDto>.Danger("Gagal mengambil statistik", ex);
            }
        }

        public async Task<ApiResponse<bool>> CanAddReferralAsync(string rootUserId, string referredUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Cannot refer yourself
                if (rootUserId == referredUserId)
                {
                    return ApiResponse<bool>.Success(false);
                }

                // Check if referred user already has a root
                var existingRoot = await _unitOfWork.Repository<ReferralTree, int>().Query()
                    .AnyAsync(r => r.ReferredUserId == referredUserId && !r.IsDeleted, cancellationToken);

                if (existingRoot)
                {
                    return ApiResponse<bool>.Success(false);
                }

                // Check max level for this root
                var maxLevel = await _unitOfWork.Repository<ReferralTree, int>().Query()
                    .Where(r => r.RootUserId == rootUserId && !r.IsDeleted)
                    .MaxAsync(r => (int?)r.Level, cancellationToken) ?? 0;

                if (maxLevel >= 3)
                {
                    return ApiResponse<bool>.Success(false);
                }

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if can add referral");
                return ApiResponse<bool>.Danger("Gagal mengecek referral", ex);
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Repository<ReferralTree, int>().ExistsAsync(id, cancellationToken);
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<bool>> ReferralExistsAsync(string rootUserId, string referredUserId, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Repository<ReferralTree, int>().Query()
                .AnyAsync(r => r.RootUserId == rootUserId && r.ReferredUserId == referredUserId && !r.IsDeleted, cancellationToken);
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<int>> CountAsync(ReferralTreeFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            var query = BuildFilteredQuery(filter);
            var count = await query.CountAsync(cancellationToken);
            return ApiResponse<int>.Success(count);
        }

        #region Private Methods

        private IQueryable<ReferralTree> BuildFilteredQuery(ReferralTreeFilterDto? filter)
        {
            var repository = _unitOfWork.Repository<ReferralTree, int>();
            var query = repository.Query()
                .Include(r => r.RootUser)
                .Include(r => r.ReferredUser)
                .AsQueryable();

            // Role-based filtering
            if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin() && !_currentUserService.IsBranchAdmin())
            {
                // Regular user hanya lihat referral yang terkait dengan mereka
                var userId = _currentUserService.UserId;
                query = query.Where(r => r.RootUserId == userId || r.ReferredUserId == userId);
            }
            else if (_currentUserService.IsBranchAdmin() && !_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
            {
                // BranchAdmin hanya lihat referral user di branch yang sama
                var branchId = _currentUserService.BranchId;
                query = query.Where(r => r.RootUser!.BranchId == branchId || r.ReferredUser!.BranchId == branchId);
            }

            // Apply filters
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.RootUserId))
                {
                    query = query.Where(r => r.RootUserId == filter.RootUserId);
                }

                if (!string.IsNullOrWhiteSpace(filter.ReferredUserId))
                {
                    query = query.Where(r => r.ReferredUserId == filter.ReferredUserId);
                }

                if (filter.Level.HasValue)
                {
                    query = query.Where(r => r.Level == filter.Level.Value);
                }

                if (filter.MaxLevel.HasValue)
                {
                    query = query.Where(r => r.Level <= filter.MaxLevel.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(r => r.IsActive == filter.IsActive.Value);
                }

                // Apply sorting
                if (!string.IsNullOrWhiteSpace(filter.SortBy))
                {
                    query = filter.SortBy.ToLower() switch
                    {
                        "level" => filter.SortDescending ? query.OrderByDescending(r => r.Level) : query.OrderBy(r => r.Level),
                        "created" => filter.SortDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
                        _ => filter.SortDescending ? query.OrderByDescending(r => r.Level) : query.OrderBy(r => r.Level)
                    };
                }
                else
                {
                    query = query.OrderBy(r => r.Level).ThenBy(r => r.CreatedAt);
                }
            }
            else
            {
                query = query.OrderBy(r => r.Level).ThenBy(r => r.CreatedAt);
            }

            return query;
        }

        private bool CanAccessReferral(ReferralTree referral)
        {
            // SuperAdmin dan Admin bisa akses semua
            if (_currentUserService.CanAccessAllData())
                return true;

            // BranchAdmin bisa akses referral user di branch yang sama
            if (_currentUserService.IsBranchAdmin())
            {
                var branchId = _currentUserService.BranchId;
                if (referral.RootUser?.BranchId == branchId || referral.ReferredUser?.BranchId == branchId)
                    return true;
            }

            // User bisa akses referral yang terkait dengan mereka
            if (_currentUserService.UserId == referral.RootUserId ||
                _currentUserService.UserId == referral.ReferredUserId ||
                _currentUserService.UserId == referral.ParentUserId)
                return true;

            return false;
        }

        private TreeNodeDto<ReferralTreeNodeData>? BuildTreeStructure(string rootUserId, List<ReferralTree> referrals)
        {
            // Find root user info
            var rootReferral = referrals.FirstOrDefault(r => r.ReferredUserId == rootUserId);
            var rootUser = rootReferral?.ReferredUser;

            if (rootUser == null)
            {
                // Try to get root user from database
                rootUser = _unitOfWork.Repository<ApplicationUser, string>().QueryIgnoreFilters()
                    .FirstOrDefault(u => u.Id == rootUserId);
            }

            if (rootUser == null)
                return null;

            var rootNode = new TreeNodeDto<ReferralTreeNodeData>
            {
                Id = rootUserId,
                Data = new ReferralTreeNodeData
                {
                    UserId = rootUserId,
                    UserName = rootUser.UserName,
                    Email = rootUser.Email,
                    FullName = rootUser.FullName,
                    CommissionPercent = 0,
                    JoinedAt = rootUser.CreatedAt,
                    IsActive = rootUser.IsActive,
                    DirectReferrals = referrals.Count(r => r.ParentUserId == rootUserId),
                    TotalDescendants = referrals.Count
                },
                Level = 0,
                Children = new List<TreeNodeDto<ReferralTreeNodeData>>()
            };

            // Build children recursively
            BuildChildren(rootNode, referrals, 1);

            return rootNode;
        }

        private void BuildChildren(TreeNodeDto<ReferralTreeNodeData> parentNode, List<ReferralTree> allReferrals, int currentLevel)
        {
            if (currentLevel > 3) return;

            var children = allReferrals
                .Where(r => r.ParentUserId == parentNode.Id && r.Level == currentLevel)
                .ToList();

            foreach (var child in children)
            {
                var childNode = new TreeNodeDto<ReferralTreeNodeData>
                {
                    Id = child.ReferredUserId,
                    Data = new ReferralTreeNodeData
                    {
                        UserId = child.ReferredUserId,
                        UserName = child.ReferredUser?.UserName,
                        Email = child.ReferredUser?.Email,
                        FullName = child.ReferredUser?.FullName,
                        CommissionPercent = child.CommissionPercent,
                        JoinedAt = child.CreatedAt,
                        IsActive = child.IsActive,
                        DirectReferrals = allReferrals.Count(r => r.ParentUserId == child.ReferredUserId),
                        TotalDescendants = CountDescendants(child.ReferredUserId, allReferrals)
                    },
                    Level = currentLevel,
                    ParentId = parentNode.Id,
                    Children = new List<TreeNodeDto<ReferralTreeNodeData>>()
                };

                parentNode.Children.Add(childNode);

                // Recursively build grandchildren
                BuildChildren(childNode, allReferrals, currentLevel + 1);
            }
        }

        private int CountDescendants(string userId, List<ReferralTree> allReferrals)
        {
            var count = 0;
            var children = allReferrals.Where(r => r.ParentUserId == userId).ToList();

            foreach (var child in children)
            {
                count++;
                count += CountDescendants(child.ReferredUserId, allReferrals);
            }

            return count;
        }

        private ReferralTreeDto MapToDto(ReferralTree referral)
        {
            var dto = _mapper.Map<ReferralTreeDto>(referral);

            if (referral.RootUser != null)
            {
                dto.RootUserName = referral.RootUser.UserName;
                dto.RootUserEmail = referral.RootUser.Email;
            }

            if (referral.ReferredUser != null)
            {
                dto.ReferredUserName = referral.ReferredUser.UserName;
                dto.ReferredUserEmail = referral.ReferredUser.Email;
            }

            if (referral.ParentUser != null)
            {
                dto.ParentUserName = referral.ParentUser.UserName;
            }

            return dto;
        }

        #endregion
        #region Private Helper Methods (New)
        private async Task UpdateDescendantsLevelRecursiveAsync(string parentUserId, int parentLevel, string rootUserId, CancellationToken cancellationToken)
        {
            var children = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                .Where(r => r.ParentUserId == parentUserId && !r.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var child in children)
            {
                child.Level = parentLevel + 1;
                child.RootUserId = rootUserId;
                child.CommissionPercent = child.Level switch
                {
                    1 => 10m,
                    2 => 5m,
                    3 => 2.5m,
                    _ => 0
                };

                child.MarkAsUpdated("System");
                _unitOfWork.Repository<ReferralTree, int>().Update(child);

                if (child.Level < 3)
                {
                    await UpdateDescendantsLevelRecursiveAsync(child.ReferredUserId, child.Level, rootUserId, cancellationToken);
                }
            }
        }
        private async Task SoftDeleteReferralNodeAsync(ReferralTree node, string? deletedBy, CancellationToken cancellationToken)
        {
            node.SoftDelete(deletedBy ?? "System");
            _unitOfWork.Repository<ReferralTree, int>().Update(node);
        }

        #endregion
        #region New Advanced Features (Move & Auto-Promote)

        public async Task<ApiResponse<bool>> MoveDownlineAsync(string userIdToMove, string newParentUserId, CancellationToken cancellationToken = default)
        {
            // Hapus 'using var transaction' karena BeginTransactionAsync tidak return object
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Validasi input tidak boleh kosong atau sama
                if (string.IsNullOrWhiteSpace(userIdToMove) || string.IsNullOrWhiteSpace(newParentUserId))
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<bool>.ValidationError(new List<ErrorDetail> { new() { Message = "User ID dan Parent ID wajib diisi" } });
                }

                if (userIdToMove == newParentUserId)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<bool>.Error("Tidak dapat memindahkan ke diri sendiri", "Invalid Operation", 400);
                }

                // 2. Cek keberadaan user yang dipindahkan
                var userToMove = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                    .FirstOrDefaultAsync(r => r.ReferredUserId == userIdToMove && !r.IsDeleted, cancellationToken);

                if (userToMove == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<bool>.NotFound("User yang akan dipindahkan tidak ditemukan dalam tree");
                }

                // 3. Cek keberadaan target parent baru
                var newParent = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                    .FirstOrDefaultAsync(r => r.ReferredUserId == newParentUserId && !r.IsDeleted, cancellationToken);

                if (newParent == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<bool>.NotFound("Target upline baru tidak ditemukan");
                }

                // 4. Anti-Looping: Cek apakah target adalah descendant dari user yang mau dipindahkan
                var isTargetDescendant = await IsDescendantAsync(userIdToMove, newParentUserId, cancellationToken);
                if (isTargetDescendant)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<bool>.Error("Tidak dapat memindahkan ke downline sendiri (akan menyebabkan looping)", "Cyclic Reference", 400);
                }

                // 5. Anti-Double: Cek apakah user sudah ada di tree target
                var existingInTargetTree = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                    .AnyAsync(r => r.RootUserId == newParent.RootUserId && r.ReferredUserId == userIdToMove && r.Id != userToMove.Id && !r.IsDeleted, cancellationToken);

                if (existingInTargetTree)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<bool>.Error("User sudah terdaftar dalam tree target", "Duplicate Entry", 409);
                }

                // 6. Cek level constraint (max level 3)
                var newLevel = newParent.Level + 1;
                if (newLevel > 3)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<bool>.Error("Level maksimal tercapai (3), tidak dapat menambahkan downline lagi", "Max Level Reached", 400);
                }

                var oldParentId = userToMove.ParentUserId;
                var oldRootId = userToMove.RootUserId;

                // 7. Update parent dan root
                userToMove.ParentUserId = newParentUserId;
                userToMove.RootUserId = newParent.RootUserId;
                userToMove.Level = newLevel;

                // Update commission sesuai level baru
                userToMove.CommissionPercent = newLevel switch
                {
                    1 => 10.0m,
                    2 => 5.0m,
                    3 => 2.5m,
                    _ => 0
                };

                userToMove.MarkAsUpdated(_currentUserService.UserId ?? "System");
                _unitOfWork.Repository<ReferralTree, int>().Update(userToMove);

                // 8. Update semua descendant secara recursive
                await UpdateDescendantsLevelRecursiveAsync(userIdToMove, newLevel, newParent.RootUserId, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("User {UserId} dipindahkan dari {OldParent} ke {NewParent}",
                    userIdToMove, oldParentId, newParentUserId);

                return ApiResponse<bool>.Success(true, $"Berhasil memindahkan user ke level {newLevel}");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error moving downline {UserId} to {NewParent}", userIdToMove, newParentUserId);
                return ApiResponse<bool>.Danger("Gagal memindahkan downline", ex);
            }
        }

        public async Task<ApiResponse<bool>> AutoPromoteDownlinesAsync(string deletedUserId, string? deletedBy = null, CancellationToken cancellationToken = default)
        {
            // Hapus 'using var transaction'
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Cek data user yang dihapus
                var deletedUserNode = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                    .FirstOrDefaultAsync(r => r.ReferredUserId == deletedUserId && !r.IsDeleted, cancellationToken);

                if (deletedUserNode == null)
                {
                    await _unitOfWork.CommitTransactionAsync(cancellationToken); // Commit kosong
                    return ApiResponse<bool>.Success(true, "Tidak ada downline yang perlu dipromosikan");
                }

                var parentOfDeleted = deletedUserNode.ParentUserId;
                var oldLevel = deletedUserNode.Level;

                // 2. Ambil semua direct downline
                var directDownlines = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                    .Where(r => r.ParentUserId == deletedUserId && !r.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!directDownlines.Any())
                {
                    // Tidak punya downline, langsung soft delete node ini saja
                    await SoftDeleteReferralNodeAsync(deletedUserNode, deletedBy, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return ApiResponse<bool>.Success(true, "User dihapus, tidak ada downline untuk dipromosikan");
                }

                // 3. Promosi massal
                foreach (var downline in directDownlines)
                {
                    downline.ParentUserId = parentOfDeleted;
                    downline.Level = oldLevel; // Naik 1 level (angka berkurang)

                    if (string.IsNullOrEmpty(parentOfDeleted))
                    {
                        downline.RootUserId = downline.ReferredUserId; // Jadi root
                        downline.Level = 0;
                    }
                    else
                    {
                        var newParent = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                            .FirstOrDefaultAsync(r => r.ReferredUserId == parentOfDeleted, cancellationToken);
                        downline.RootUserId = newParent?.RootUserId ?? downline.ReferredUserId;
                    }

                    downline.CommissionPercent = downline.Level switch
                    {
                        0 => 0m,
                        1 => 10m,
                        2 => 5m,
                        3 => 2.5m,
                        _ => 0
                    };

                    downline.MarkAsUpdated(deletedBy ?? "System");
                    _unitOfWork.Repository<ReferralTree, int>().Update(downline);

                    _logger.LogInformation("Promoting user {UserId} ke Level {NewLevel}",
                        downline.ReferredUserId, downline.Level);

                    // 4. Update semua descendant
                    await UpdateDescendantsLevelRecursiveAsync(downline.ReferredUserId, downline.Level, downline.RootUserId, cancellationToken);
                }

                // 5. Soft delete node user yang dihapus
                await SoftDeleteReferralNodeAsync(deletedUserNode, deletedBy, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, $"{directDownlines.Count} downline berhasil dipromosikan");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error auto-promoting downlines for {UserId}", deletedUserId);
                return ApiResponse<bool>.Danger("Gagal mempromosikan downline", ex);
            }
        }

        public async Task<ApiResponse<bool>> AssignOrphanUserAsync(string orphanUserId, string targetParentUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Cek apakah user benar-benar orphan (tidak punya referral tree)
                var existingReferral = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                    .AnyAsync(r => r.ReferredUserId == orphanUserId && !r.IsDeleted, cancellationToken);

                if (existingReferral)
                    return ApiResponse<bool>.Error("User sudah memiliki posisi dalam tree", "Already Assigned", 409);

                // 2. Validasi target parent
                var parent = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                    .FirstOrDefaultAsync(r => r.ReferredUserId == targetParentUserId && !r.IsDeleted, cancellationToken);

                if (parent == null)
                    return ApiResponse<bool>.NotFound("Target upline tidak ditemukan");

                // 3. Cek level constraint
                var newLevel = parent.Level + 1;
                if (newLevel > 3)
                    return ApiResponse<bool>.Error("Level maksimal tercapai", "Max Level", 400);

                // 4. Cek apakah user exist di Identity
                var userExists = await _unitOfWork.Repository<ApplicationUser, string>().ExistsAsync(orphanUserId);
                if (!userExists)
                    return ApiResponse<bool>.NotFound("User tidak ditemukan dalam sistem");

                // 5. Create new referral entry
                var newReferral = new ReferralTree
                {
                    RootUserId = parent.RootUserId,
                    ReferredUserId = orphanUserId,
                    ParentUserId = targetParentUserId,
                    Level = newLevel,
                    CommissionPercent = newLevel switch { 1 => 10m, 2 => 5m, 3 => 2.5m, _ => 0 },
                    IsActive = true,
                    IsDeleted = false
                };
                newReferral.MarkAsCreated(_currentUserService.UserId ?? "System");

                await _unitOfWork.Repository<ReferralTree, int>().AddAsync(newReferral, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, $"User berhasil ditempatkan di Level {newLevel}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning orphan user {OrphanId} to {ParentId}", orphanUserId, targetParentUserId);
                return ApiResponse<bool>.Danger("Gagal menempatkan user", ex);
            }
        }

        public async Task<bool> IsDescendantAsync(string potentialAncestorId, string potentialDescendantId, CancellationToken cancellationToken = default)
        {
            if (potentialAncestorId == potentialDescendantId) return false;

            var currentId = potentialDescendantId;
            var visited = new HashSet<string>();
            int maxDepth = 10; // Safety limit

            for (int i = 0; i < maxDepth; i++)
            {
                var parent = await _unitOfWork.Repository<ReferralTree, int>().QueryIgnoreFilters()
                    .FirstOrDefaultAsync(r => r.ReferredUserId == currentId && !r.IsDeleted, cancellationToken);

                if (parent == null) break;

                if (parent.ParentUserId == potentialAncestorId)
                    return true;

                if (visited.Contains(parent.ParentUserId ?? ""))
                    break; // Loop detected

                visited.Add(currentId);
                currentId = parent.ParentUserId ?? "";

                if (string.IsNullOrEmpty(currentId)) break;
            }

            return false;
        }
        public async Task<ApiResponse<List<ReferralTreeNodeDto>>> GetDirectDownlinesAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var downlines = await _unitOfWork.Repository<ReferralTree, int>().Query()
                    .Include(r => r.ReferredUser)
                    .Where(r => r.ParentUserId == userId && !r.IsDeleted && r.IsActive)
                    .OrderBy(r => r.Level)
                    .ThenBy(r => r.CreatedAt)
                    .ToListAsync(cancellationToken);

                var dtos = downlines.Select(r => new ReferralTreeNodeDto
                {
                    UserId = r.ReferredUserId,
                    UserName = r.ReferredUser?.UserName,
                    Email = r.ReferredUser?.Email,
                    FullName = r.ReferredUser?.FullName,
                    Level = r.Level,
                    ParentUserId = r.ParentUserId,
                    CommissionPercent = r.CommissionPercent,
                    JoinedAt = r.CreatedAt,
                    IsActive = r.IsActive
                }).ToList();

                return ApiResponse<List<ReferralTreeNodeDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting direct downlines for {UserId}", userId);
                return ApiResponse<List<ReferralTreeNodeDto>>.Danger("Gagal mengambil data downline", ex);
            }
        }

        #endregion
    }
}