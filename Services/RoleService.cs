// RoleService.cs
using ApiGMPKlik.Application.Interfaces;
using ApiGMPKlik.DTOs;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Interfaces.Repositories;
using ApiGMPKlik.Models;
using ApiGMPKlik.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiGMPKlik.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;  // <-- TAMBAHKAN INI

        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IUnitOfWork unitOfWork,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,  // <-- TAMBAHKAN INI
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<RoleService> logger)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;  // <-- TAMBAHKAN INI
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ApiResponse<RoleDto>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null || role.IsDeleted)
                    return ApiResponse<RoleDto>.NotFound("Role");

                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<RoleDto>.Forbidden("Akses ditolak");

                var dto = await MapToDtoAsync(role);
                return ApiResponse<RoleDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by id: {Id}", id);
                return ApiResponse<RoleDto>.Danger("Gagal mengambil data role", ex);
            }
        }

        public async Task<ApiResponse<RoleDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(name);
                if (role == null || role.IsDeleted)
                    return ApiResponse<RoleDto>.NotFound("Role");

                var dto = await MapToDtoAsync(role);
                return ApiResponse<RoleDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by name: {Name}", name);
                return ApiResponse<RoleDto>.Danger("Gagal mengambil data role", ex);
            }
        }

        public async Task<ApiResponse<List<RoleDto>>> GetAllAsync(RoleFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = BuildFilteredQuery(filter);
                var roles = await query.ToListAsync(cancellationToken);
                var dtos = await Task.WhenAll(roles.Select(r => MapToDtoAsync(r)));
                return ApiResponse<List<RoleDto>>.Success(dtos.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return ApiResponse<List<RoleDto>>.Danger("Gagal mengambil data role", ex);
            }
        }

        public async Task<ApiResponse<PaginatedList<RoleDto>>> GetPaginatedAsync(RoleFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = BuildFilteredQuery(filter);
                var totalCount = await query.CountAsync(cancellationToken);

                var roles = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var dtos = await Task.WhenAll(roles.Select(r => MapToDtoAsync(r)));

                var result = new PaginatedList<RoleDto>
                {
                    Items = dtos.ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasNext = page * pageSize < totalCount,
                    HasPrevious = page > 1
                };

                return ApiResponse<PaginatedList<RoleDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated roles");
                return ApiResponse<PaginatedList<RoleDto>>.Danger("Gagal mengambil data role", ex);
            }
        }

        public async Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<RoleDto>.Forbidden("Hanya Admin yang dapat membuat role");

                if (await _roleManager.RoleExistsAsync(dto.Name))
                    return ApiResponse<RoleDto>.Error("Role sudah ada", "Duplicate", 409);

                var role = new ApplicationRole
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    SortOrder = dto.SortOrder,
                    NormalizedName = dto.Name.ToUpper()
                };

                role.MarkAsCreated(_currentUserService.UserId ?? "System");

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                    return ApiResponse<RoleDto>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "CreationFailed", 400);

                var resultDto = await MapToDtoAsync(role);
                return ApiResponse<RoleDto>.Created(resultDto, "Role berhasil dibuat");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return ApiResponse<RoleDto>.Danger("Gagal membuat role", ex);
            }
        }

        public async Task<ApiResponse<RoleDto>> UpdateAsync(string id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<RoleDto>.Forbidden("Hanya Admin yang dapat mengupdate role");

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null || role.IsDeleted)
                    return ApiResponse<RoleDto>.NotFound("Role");

                role.Description = dto.Description;
                role.SortOrder = dto.SortOrder;
                role.MarkAsUpdated(_currentUserService.UserId ?? "System");

                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                    return ApiResponse<RoleDto>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "UpdateFailed", 400);

                var resultDto = await MapToDtoAsync(role);
                return ApiResponse<RoleDto>.Success(resultDto, "Role berhasil diupdate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {Id}", id);
                return ApiResponse<RoleDto>.Danger("Gagal mengupdate role", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat menghapus role");

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                    return ApiResponse<bool>.NotFound("Role");

                // Check if role has users
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                if (usersInRole.Any(u => !u.IsDeleted))
                    return ApiResponse<bool>.Error("Role masih memiliki user aktif", "HasActiveUsers", 400);

                role.SoftDelete(_currentUserService.UserId ?? "System");
                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                    return ApiResponse<bool>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "DeleteFailed", 400);

                return ApiResponse<bool>.Success(true, "Role berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal menghapus role", ex);
            }
        }

        public async Task<ApiResponse<bool>> RestoreAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya SuperAdmin yang dapat restore role");

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                    return ApiResponse<bool>.NotFound("Role");

                if (!role.IsDeleted)
                    return ApiResponse<bool>.Error("Role tidak dalam status terhapus", "NotDeleted", 400);

                role.Restore(_currentUserService.UserId ?? "System");
                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                    return ApiResponse<bool>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "RestoreFailed", 400);

                return ApiResponse<bool>.Success(true, "Role berhasil direstore");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring role: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal restore role", ex);
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            var exists = await _roleManager.RoleExistsAsync(name);
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<int>> CountAsync(RoleFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            var query = BuildFilteredQuery(filter);
            var count = await query.CountAsync(cancellationToken);
            return ApiResponse<int>.Success(count);
        }

        public async Task<ApiResponse<List<PermissionDto>>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var permissions = await _unitOfWork.Repository<RolePermission, int>().Query()
                    .Where(rp => rp.RoleId == roleId && !rp.Permission.IsDeleted)
                    .Include(rp => rp.Permission)
                    .Select(rp => rp.Permission)
                    .ToListAsync(cancellationToken);

                var dtos = _mapper.Map<List<PermissionDto>>(permissions);
                return ApiResponse<List<PermissionDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role permissions");
                return ApiResponse<List<PermissionDto>>.Danger("Gagal mengambil permissions", ex);
            }
        }

        private IQueryable<ApplicationRole> BuildFilteredQuery(RoleFilterDto? filter)
        {
            var query = _roleManager.Roles.AsQueryable();

            if (filter == null)
                return query.Where(r => !r.IsDeleted);

            if (!_currentUserService.IsSuperAdmin())
                query = query.Where(r => !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(r =>
                    r.Name!.ToLower().Contains(search) ||
                    (r.Description != null && r.Description.ToLower().Contains(search)));
            }

            if (filter.IsActive.HasValue)
            {
                if (filter.IsActive.Value)
                    query = query.Where(r => !r.IsDeleted);
                else
                    query = query.Where(r => r.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "name" => filter.SortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                    "sortorder" => filter.SortDescending ? query.OrderByDescending(r => r.SortOrder) : query.OrderBy(r => r.SortOrder),
                    _ => query.OrderBy(r => r.SortOrder).ThenBy(r => r.Name)
                };
            }
            else
            {
                query = query.OrderBy(r => r.SortOrder).ThenBy(r => r.Name);
            }

            return query;
        }

        private async Task<RoleDto> MapToDtoAsync(ApplicationRole role)
        {
            var userCount = await _userManager.GetUsersInRoleAsync(role.Name!);
            var permCount = await _unitOfWork.Repository<RolePermission, int>().Query()
                .CountAsync(rp => rp.RoleId == role.Id);

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                NormalizedName = role.NormalizedName,
                Description = role.Description,
                SortOrder = role.SortOrder,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy,
                ModifiedAt = role.ModifiedAt,
                IsDeleted = role.IsDeleted,
                UserCount = userCount.Count(u => !u.IsDeleted),
                PermissionCount = permCount
            };
        }
    }

    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<ApplicationRole> _roleManager;  // ← TAMBAHKAN
        private readonly UserManager<ApplicationUser> _userManager;  // ← TAMBAHKAN
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(
            IUnitOfWork unitOfWork,
            RoleManager<ApplicationRole> roleManager,      // ← TAMBAHKAN
            UserManager<ApplicationUser> userManager,      // ← TAMBAHKAN
            ICurrentUserService currentUserService,
            IMapper mapper,
            ILogger<PermissionService> logger)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;    // ← TAMBAHKAN
            _userManager = userManager;    // ← TAMBAHKAN
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<PermissionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var repo = _unitOfWork.Repository<Permission, int>();
                Permission? perm;

                if (_currentUserService.IsSuperAdmin())
                    perm = await repo.GetByIdIgnoreFiltersAsync(id, cancellationToken);
                else
                    perm = await repo.GetByIdAsync(id, cancellationToken);

                if (perm == null)
                    return ApiResponse<PermissionDto>.NotFound("Permission");

                var dto = await MapToDtoAsync(perm);
                return ApiResponse<PermissionDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission by id: {Id}", id);
                return ApiResponse<PermissionDto>.Danger("Gagal mengambil permission", ex);
            }
        }

        public async Task<ApiResponse<PermissionDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            try
            {
                var repo = _unitOfWork.Repository<Permission, int>();
                var query = _currentUserService.IsSuperAdmin() ? repo.QueryIgnoreFilters() : repo.Query();

                var perm = await query.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
                if (perm == null)
                    return ApiResponse<PermissionDto>.NotFound("Permission");

                var dto = await MapToDtoAsync(perm);
                return ApiResponse<PermissionDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission by code: {Code}", code);
                return ApiResponse<PermissionDto>.Danger("Gagal mengambil permission", ex);
            }
        }

        public async Task<ApiResponse<List<PermissionDto>>> GetAllAsync(PermissionFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = BuildFilteredQuery(filter);
                var perms = await query.ToListAsync(cancellationToken);
                var dtos = await Task.WhenAll(perms.Select(p => MapToDtoAsync(p)));
                return ApiResponse<List<PermissionDto>>.Success(dtos.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions");
                return ApiResponse<List<PermissionDto>>.Danger("Gagal mengambil permissions", ex);
            }
        }

        public async Task<ApiResponse<PaginatedList<PermissionDto>>> GetPaginatedAsync(PermissionFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = BuildFilteredQuery(filter);
                var totalCount = await query.CountAsync(cancellationToken);

                var perms = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var dtos = await Task.WhenAll(perms.Select(p => MapToDtoAsync(p)));

                var result = new PaginatedList<PermissionDto>
                {
                    Items = dtos.ToList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasNext = page * pageSize < totalCount,
                    HasPrevious = page > 1
                };

                return ApiResponse<PaginatedList<PermissionDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated permissions");
                return ApiResponse<PaginatedList<PermissionDto>>.Danger("Gagal mengambil permissions", ex);
            }
        }

        public async Task<ApiResponse<PermissionDto>> CreateAsync(CreatePermissionDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<PermissionDto>.Forbidden("Hanya Admin yang dapat membuat permission");

                var repo = _unitOfWork.Repository<Permission, int>();

                // Check duplicate code
                var exists = await repo.QueryIgnoreFilters()
                    .AnyAsync(p => p.Code == dto.Code, cancellationToken);

                if (exists)
                    return ApiResponse<PermissionDto>.Error("Kode permission sudah digunakan", "DuplicateCode", 409);

                var perm = _mapper.Map<Permission>(dto);
                await repo.AddAsync(perm, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var result = await MapToDtoAsync(perm);
                return ApiResponse<PermissionDto>.Created(result, "Permission berhasil dibuat");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return ApiResponse<PermissionDto>.Danger("Gagal membuat permission", ex);
            }
        }

        public async Task<ApiResponse<PermissionDto>> UpdateAsync(int id, UpdatePermissionDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<PermissionDto>.Forbidden("Hanya Admin yang dapat mengupdate permission");

                var repo = _unitOfWork.Repository<Permission, int>();
                var perm = await repo.GetByIdIgnoreFiltersAsync(id, cancellationToken);

                if (perm == null)
                    return ApiResponse<PermissionDto>.NotFound("Permission");

                _mapper.Map(dto, perm);
                repo.Update(perm);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var result = await MapToDtoAsync(perm);
                return ApiResponse<PermissionDto>.Success(result, "Permission berhasil diupdate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission: {Id}", id);
                return ApiResponse<PermissionDto>.Danger("Gagal mengupdate permission", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat menghapus permission");

                var repo = _unitOfWork.Repository<Permission, int>();
                var perm = await repo.GetByIdAsync(id, cancellationToken);

                if (perm == null)
                    return ApiResponse<bool>.NotFound("Permission");

                repo.SoftDelete(perm, _currentUserService.UserId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Permission berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal menghapus permission", ex);
            }
        }

        public async Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya SuperAdmin yang dapat restore");

                var repo = _unitOfWork.Repository<Permission, int>();
                var perm = await repo.GetByIdIgnoreFiltersAsync(id, cancellationToken);

                if (perm == null)
                    return ApiResponse<bool>.NotFound("Permission");

                if (!perm.IsDeleted)
                    return ApiResponse<bool>.Error("Permission tidak dalam status terhapus", "NotDeleted", 400);

                repo.Restore(perm, _currentUserService.UserId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Permission berhasil direstore");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring permission: {Id}", id);
                return ApiResponse<bool>.Danger("Gagal restore permission", ex);
            }
        }

        public async Task<ApiResponse<bool>> ExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            var exists = await _unitOfWork.Repository<Permission, int>().QueryIgnoreFilters()
                .AnyAsync(p => p.Code == code, cancellationToken);
            return ApiResponse<bool>.Success(exists);
        }

        public async Task<ApiResponse<int>> CountAsync(PermissionFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            var query = BuildFilteredQuery(filter);
            var count = await query.CountAsync(cancellationToken);
            return ApiResponse<int>.Success(count);
        }

        public async Task<ApiResponse<List<string>>> GetModulesAsync(CancellationToken cancellationToken = default)
        {
            var modules = await _unitOfWork.Repository<Permission, int>().Query()
                .Where(p => !p.IsDeleted)
                .Select(p => p.Module)
                .Distinct()
                .ToListAsync(cancellationToken);

            return ApiResponse<List<string>>.Success(modules);
        }

        private IQueryable<Permission> BuildFilteredQuery(PermissionFilterDto? filter)
        {
            var repo = _unitOfWork.Repository<Permission, int>();
            IQueryable<Permission> query;

            if (_currentUserService.IsSuperAdmin())
                query = repo.QueryIgnoreFilters();
            else
                query = repo.Query();

            if (filter == null) return query;

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Code.ToLower().Contains(search) ||
                    (p.Description != null && p.Description.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Module))
                query = query.Where(p => p.Module == filter.Module);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "code" => filter.SortDescending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
                    "name" => filter.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "module" => filter.SortDescending ? query.OrderByDescending(p => p.Module) : query.OrderBy(p => p.Module),
                    _ => query.OrderBy(p => p.Module).ThenBy(p => p.SortOrder)
                };
            }
            else
            {
                query = query.OrderBy(p => p.Module).ThenBy(p => p.SortOrder);
            }

            return query;
        }

        private async Task<PermissionDto> MapToDtoAsync(Permission perm)
        {
            var roleCount = await _unitOfWork.Repository<RolePermission, int>().Query()
                .CountAsync(rp => rp.PermissionId == perm.Id);

            return new PermissionDto
            {
                Id = perm.Id,
                Code = perm.Code,
                Name = perm.Name,
                Description = perm.Description,
                Module = perm.Module,
                SortOrder = perm.SortOrder,
                IsActive = perm.IsActive,
                IsDeleted = perm.IsDeleted,
                CreatedAt = perm.CreatedAt,
                RoleCount = roleCount
            };
        }
        public async Task SyncUserPermissionsToClaimsAsync(string userId, System.Security.Claims.ClaimsIdentity identity)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return;

                var roles = await _userManager.GetRolesAsync(user);
                var permissions = new List<string>();

                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        var rolePerms = await _unitOfWork.Repository<RolePermission, int>().Query()
                            .Where(rp => rp.RoleId == role.Id && !rp.Permission.IsDeleted)
                            .Select(rp => rp.Permission.Code)
                            .ToListAsync();
                        permissions.AddRange(rolePerms);
                    }
                }

                // Tambahkan permission claims
                foreach (var perm in permissions.Distinct())
                {
                    if (!identity.HasClaim(c => c.Type == "Permission" && c.Value == perm))
                    {
                        identity.AddClaim(new System.Security.Claims.Claim("Permission", perm));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing permissions for user {UserId}", userId);
            }
        }
    }
    public class UserLoginService : IUserLoginService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserLoginService> _logger;

        public UserLoginService(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            ILogger<UserLoginService> logger)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<UserLoginDto>> CreateAsync(CreateUserLoginDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.CanAccessAllData() && _currentUserService.UserId != dto.UserId)
                    return ApiResponse<UserLoginDto>.Forbidden("Akses ditolak");

                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null)
                    return ApiResponse<UserLoginDto>.NotFound("User");

                var loginInfo = new UserLoginInfo(dto.LoginProvider, dto.ProviderKey, dto.ProviderDisplayName);
                var result = await _userManager.AddLoginAsync(user, loginInfo);

                if (!result.Succeeded)
                    return ApiResponse<UserLoginDto>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "AddFailed", 400);

                return ApiResponse<UserLoginDto>.Success(new UserLoginDto
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    LoginProvider = dto.LoginProvider,
                    ProviderKey = dto.ProviderKey,
                    ProviderDisplayName = dto.ProviderDisplayName
                }, "Login berhasil ditambahkan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user login");
                return ApiResponse<UserLoginDto>.Danger("Gagal menambah login", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.CanAccessAllData() && _currentUserService.UserId != userId)
                    return ApiResponse<bool>.Forbidden("Akses ditolak");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<bool>.NotFound("User");

                var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
                if (!result.Succeeded)
                    return ApiResponse<bool>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "RemoveFailed", 400);

                return ApiResponse<bool>.Success(true, "Login berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user login");
                return ApiResponse<bool>.Danger("Gagal menghapus login", ex);
            }
        }

        public async Task<ApiResponse<List<UserLoginDto>>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.CanAccessAllData() && _currentUserService.UserId != userId)
                    return ApiResponse<List<UserLoginDto>>.Forbidden("Akses ditolak");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<List<UserLoginDto>>.NotFound("User");

                var logins = await _userManager.GetLoginsAsync(user);
                var dtos = logins.Select(l => new UserLoginDto
                {
                    UserId = userId,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    LoginProvider = l.LoginProvider,
                    ProviderKey = l.ProviderKey,
                    ProviderDisplayName = l.ProviderDisplayName
                }).ToList();

                return ApiResponse<List<UserLoginDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user logins");
                return ApiResponse<List<UserLoginDto>>.Danger("Gagal mengambil logins", ex);
            }
        }

        public async Task<ApiResponse<UserLoginDto?>> FindAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
                if (user == null)
                    return ApiResponse<UserLoginDto?>.Success(null);

                var login = (await _userManager.GetLoginsAsync(user))
                    .FirstOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);

                if (login == null)
                    return ApiResponse<UserLoginDto?>.Success(null);

                return ApiResponse<UserLoginDto?>.Success(new UserLoginDto
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    LoginProvider = login.LoginProvider,
                    ProviderKey = login.ProviderKey,
                    ProviderDisplayName = login.ProviderDisplayName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding user login");
                return ApiResponse<UserLoginDto?>.Danger("Gagal mencari login", ex);
            }
        }
    }
    public class RoleClaimService : IRoleClaimService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RoleClaimService> _logger;

        public RoleClaimService(
            RoleManager<ApplicationRole> roleManager,
            ICurrentUserService currentUserService,
            ILogger<RoleClaimService> logger)
        {
            _roleManager = roleManager;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<RoleClaimDto>> CreateAsync(CreateRoleClaimDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<RoleClaimDto>.Forbidden("Hanya Admin yang dapat menambah role claim");

                var role = await _roleManager.FindByIdAsync(dto.RoleId);
                if (role == null)
                    return ApiResponse<RoleClaimDto>.NotFound("Role");

                var claim = new Claim(dto.ClaimType, dto.ClaimValue);
                var result = await _roleManager.AddClaimAsync(role, claim);

                if (!result.Succeeded)
                    return ApiResponse<RoleClaimDto>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "AddFailed", 400);

                return ApiResponse<RoleClaimDto>.Success(new RoleClaimDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    ClaimType = dto.ClaimType,
                    ClaimValue = dto.ClaimValue
                }, "Role claim berhasil ditambahkan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role claim");
                return ApiResponse<RoleClaimDto>.Danger("Gagal menambah role claim", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return ApiResponse<bool>.Error("Gunakan RemoveClaimAsync dengan parameter RoleId, ClaimType, dan ClaimValue", "NotImplemented", 501);
        }

        public async Task<ApiResponse<List<RoleClaimDto>>> GetByRoleAsync(string roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return ApiResponse<List<RoleClaimDto>>.NotFound("Role");

                var claims = await _roleManager.GetClaimsAsync(role);
                var dtos = claims.Select(c => new RoleClaimDto
                {
                    RoleId = roleId,
                    RoleName = role.Name!,
                    ClaimType = c.Type,
                    ClaimValue = c.Value
                }).ToList();

                return ApiResponse<List<RoleClaimDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role claims");
                return ApiResponse<List<RoleClaimDto>>.Danger("Gagal mengambil role claims", ex);
            }
        }

        public async Task<ApiResponse<bool>> RemoveClaimAsync(string roleId, string claimType, string claimValue, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat menghapus role claim");

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    return ApiResponse<bool>.NotFound("Role");

                var claim = new Claim(claimType, claimValue);
                var result = await _roleManager.RemoveClaimAsync(role, claim);

                if (!result.Succeeded)
                    return ApiResponse<bool>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "RemoveFailed", 400);

                return ApiResponse<bool>.Success(true, "Role claim berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role claim");
                return ApiResponse<bool>.Danger("Gagal menghapus role claim", ex);
            }
        }
    }
    public class UserClaimService : IUserClaimService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserClaimService> _logger;

        public UserClaimService(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            ILogger<UserClaimService> logger)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<UserClaimDto>> CreateAsync(CreateUserClaimDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.CanAccessAllData() && _currentUserService.UserId != dto.UserId)
                    return ApiResponse<UserClaimDto>.Forbidden("Akses ditolak");

                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null)
                    return ApiResponse<UserClaimDto>.NotFound("User");

                var claim = new Claim(dto.ClaimType, dto.ClaimValue);
                var result = await _userManager.AddClaimAsync(user, claim);

                if (!result.Succeeded)
                    return ApiResponse<UserClaimDto>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "AddFailed", 400);

                return ApiResponse<UserClaimDto>.Success(new UserClaimDto
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    ClaimType = dto.ClaimType,
                    ClaimValue = dto.ClaimValue
                }, "Claim berhasil ditambahkan");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user claim");
                return ApiResponse<UserClaimDto>.Danger("Gagal menambah claim", ex);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Note: IdentityUserClaim uses int Id, but UserManager works with Claim objects
                // We need to find by querying all users' claims (inefficient) or use DbContext directly
                // For simplicity, using RemoveClaimAsync approach in separate method

                return ApiResponse<bool>.Error("Gunakan method RemoveClaimAsync dengan parameter UserId, ClaimType, dan ClaimValue", "NotImplemented", 501);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user claim");
                return ApiResponse<bool>.Danger("Gagal menghapus claim", ex);
            }
        }

        public async Task<ApiResponse<List<UserClaimDto>>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.CanAccessAllData() && _currentUserService.UserId != userId)
                    return ApiResponse<List<UserClaimDto>>.Forbidden("Akses ditolak");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<List<UserClaimDto>>.NotFound("User");

                var claims = await _userManager.GetClaimsAsync(user);
                var dtos = claims.Select(c => new UserClaimDto
                {
                    UserId = userId,
                    UserName = user.UserName!,
                    ClaimType = c.Type,
                    ClaimValue = c.Value
                }).ToList();

                return ApiResponse<List<UserClaimDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user claims");
                return ApiResponse<List<UserClaimDto>>.Danger("Gagal mengambil claims", ex);
            }
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, UpdateUserClaimDto dto, CancellationToken cancellationToken = default)
        {
            return ApiResponse<bool>.Error("Identity Claim tidak bisa diupdate. Hapus dan buat baru.", "NotSupported", 400);
        }

        public async Task<ApiResponse<bool>> RemoveClaimAsync(string userId, string claimType, string claimValue, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.CanAccessAllData() && _currentUserService.UserId != userId)
                    return ApiResponse<bool>.Forbidden("Akses ditolak");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<bool>.NotFound("User");

                var claim = new Claim(claimType, claimValue);
                var result = await _userManager.RemoveClaimAsync(user, claim);

                if (!result.Succeeded)
                    return ApiResponse<bool>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "RemoveFailed", 400);

                return ApiResponse<bool>.Success(true, "Claim berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user claim");
                return ApiResponse<bool>.Danger("Gagal menghapus claim", ex);
            }
        }
    }
    public class UserRoleService : IUserRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserRoleService> _logger;

        public UserRoleService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ICurrentUserService currentUserService,
            ILogger<UserRoleService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<UserRoleDto>> AssignRoleAsync(AssignRoleToUserDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<UserRoleDto>.Forbidden("Hanya Admin yang dapat assign role");

                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null || user.IsDeleted)
                    return ApiResponse<UserRoleDto>.NotFound("User");

                var role = await _roleManager.FindByNameAsync(dto.RoleName);
                if (role == null || role.IsDeleted)
                    return ApiResponse<UserRoleDto>.NotFound("Role");

                if (await _userManager.IsInRoleAsync(user, dto.RoleName))
                    return ApiResponse<UserRoleDto>.Error("User sudah memiliki role ini", "Duplicate", 409);

                var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
                if (!result.Succeeded)
                    return ApiResponse<UserRoleDto>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "AssignFailed", 400);

                return ApiResponse<UserRoleDto>.Success(new UserRoleDto
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    AssignedAt = DateTime.UtcNow
                }, "Role berhasil di-assign");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role to user");
                return ApiResponse<UserRoleDto>.Danger("Gagal assign role", ex);
            }
        }

        public async Task<ApiResponse<bool>> RemoveRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat menghapus role");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<bool>.NotFound("User");

                if (!await _userManager.IsInRoleAsync(user, roleName))
                    return ApiResponse<bool>.Error("User tidak memiliki role ini", "NotFound", 404);

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (!result.Succeeded)
                    return ApiResponse<bool>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "RemoveFailed", 400);

                return ApiResponse<bool>.Success(true, "Role berhasil dihapus dari user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role from user");
                return ApiResponse<bool>.Danger("Gagal menghapus role", ex);
            }
        }

        public async Task<ApiResponse<List<UserRoleDto>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<List<UserRoleDto>>.NotFound("User");

                // Check access - hanya admin atau user sendiri yang bisa liat
                if (!_currentUserService.CanAccessAllData() && _currentUserService.UserId != userId)
                    return ApiResponse<List<UserRoleDto>>.Forbidden("Akses ditolak");

                var roles = await _userManager.GetRolesAsync(user);
                var roleList = new List<UserRoleDto>();

                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null && !role.IsDeleted)
                    {
                        roleList.Add(new UserRoleDto
                        {
                            UserId = user.Id,
                            UserName = user.UserName!,
                            Email = user.Email!,
                            RoleId = role.Id,
                            RoleName = role.Name!,
                            AssignedAt = user.CreatedAt // Approximation
                        });
                    }
                }

                return ApiResponse<List<UserRoleDto>>.Success(roleList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles");
                return ApiResponse<List<UserRoleDto>>.Danger("Gagal mengambil roles", ex);
            }
        }

        public async Task<ApiResponse<List<UserRoleDto>>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin() && !_currentUserService.IsBranchAdmin())
                    return ApiResponse<List<UserRoleDto>>.Forbidden("Akses ditolak");

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                    return ApiResponse<List<UserRoleDto>>.NotFound("Role");

                var users = await _userManager.GetUsersInRoleAsync(roleName);

                // Filter active users only for non-superadmin
                if (!_currentUserService.IsSuperAdmin())
                    users = users.Where(u => !u.IsDeleted && u.IsActive).ToList();

                var dtos = users.Select(u => new UserRoleDto
                {
                    UserId = u.Id,
                    UserName = u.UserName!,
                    Email = u.Email!,
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    AssignedAt = u.CreatedAt
                }).ToList();

                return ApiResponse<List<UserRoleDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users in role");
                return ApiResponse<List<UserRoleDto>>.Danger("Gagal mengambil users", ex);
            }
        }

        public async Task<ApiResponse<bool>> IsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return ApiResponse<bool>.Success(false);

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            return ApiResponse<bool>.Success(isInRole);
        }

        public async Task<ApiResponse<bool>> BulkAssignAsync(string userId, List<string> roleNames, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat bulk assign");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<bool>.NotFound("User");

                // Remove existing roles first (sync mode)
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new roles
                var result = await _userManager.AddToRolesAsync(user, roleNames);
                if (!result.Succeeded)
                    return ApiResponse<bool>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "BulkAssignFailed", 400);

                return ApiResponse<bool>.Success(true, $"{roleNames.Count} role berhasil di-assign");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk assigning roles");
                return ApiResponse<bool>.Danger("Gagal bulk assign", ex);
            }
        }

        public async Task<ApiResponse<bool>> BulkRemoveAsync(string userId, List<string> roleNames, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat bulk remove");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ApiResponse<bool>.NotFound("User");

                var result = await _userManager.RemoveFromRolesAsync(user, roleNames);
                if (!result.Succeeded)
                    return ApiResponse<bool>.Error(string.Join(", ", result.Errors.Select(e => e.Description)), "BulkRemoveFailed", 400);

                return ApiResponse<bool>.Success(true, $"{roleNames.Count} role berhasil dihapus");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk removing roles");
                return ApiResponse<bool>.Danger("Gagal bulk remove", ex);
            }
        }
    }
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RolePermissionService> _logger;

        public RolePermissionService(
            IUnitOfWork unitOfWork,
            RoleManager<ApplicationRole> roleManager,
            ICurrentUserService currentUserService,
            ILogger<RolePermissionService> logger)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<RolePermissionDto>> AssignPermissionAsync(AssignPermissionToRoleDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<RolePermissionDto>.Forbidden("Hanya Admin yang dapat assign permission");

                // Validate role exists
                var role = await _roleManager.FindByIdAsync(dto.RoleId);
                if (role == null || role.IsDeleted)
                    return ApiResponse<RolePermissionDto>.NotFound("Role");

                // Validate permission exists
                var perm = await _unitOfWork.Repository<Permission, int>().GetByIdAsync(dto.PermissionId, cancellationToken);
                if (perm == null || perm.IsDeleted)
                    return ApiResponse<RolePermissionDto>.NotFound("Permission");

                // Check if already assigned
                var existing = await _unitOfWork.Repository<RolePermission, int>().Query()
                    .FirstOrDefaultAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId, cancellationToken);

                if (existing != null)
                    return ApiResponse<RolePermissionDto>.Error("Permission sudah di-assign ke role ini", "Duplicate", 409);

                var rolePerm = new RolePermission
                {
                    RoleId = dto.RoleId,
                    PermissionId = dto.PermissionId
                };
                rolePerm.MarkAsCreated(_currentUserService.UserId ?? "System");

                await _unitOfWork.Repository<RolePermission, int>().AddAsync(rolePerm, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var result = new RolePermissionDto
                {
                    Id = rolePerm.Id,
                    RoleId = role.Id,
                    RoleName = role.Name!,
                    PermissionId = perm.Id,
                    PermissionCode = perm.Code,
                    PermissionName = perm.Name,
                    Module = perm.Module,
                    AssignedAt = rolePerm.CreatedAt,
                    AssignedBy = rolePerm.CreatedBy
                };

                return ApiResponse<RolePermissionDto>.Created(result, "Permission berhasil di-assign");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permission to role");
                return ApiResponse<RolePermissionDto>.Danger("Gagal assign permission", ex);
            }
        }

        public async Task<ApiResponse<bool>> RemovePermissionAsync(string roleId, int permissionId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat menghapus assign permission");

                var repo = _unitOfWork.Repository<RolePermission, int>();
                var rolePerm = await repo.Query()
                    .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

                if (rolePerm == null)
                    return ApiResponse<bool>.NotFound("Role Permission");

                repo.HardDelete(rolePerm);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ApiResponse<bool>.Success(true, "Permission berhasil dihapus dari role");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing permission from role");
                return ApiResponse<bool>.Danger("Gagal menghapus permission", ex);
            }
        }

        public async Task<ApiResponse<bool>> BulkAssignAsync(BulkAssignPermissionDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat bulk assign");

                var role = await _roleManager.FindByIdAsync(dto.RoleId);
                if (role == null)
                    return ApiResponse<bool>.NotFound("Role");

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    var repo = _unitOfWork.Repository<RolePermission, int>();

                    // Remove existing permissions not in list (optional - sync mode)
                    // Or just add new ones (additive mode) - let's do additive

                    foreach (var permId in dto.PermissionIds)
                    {
                        var exists = await repo.Query()
                            .AnyAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == permId, cancellationToken);

                        if (!exists)
                        {
                            var rolePerm = new RolePermission
                            {
                                RoleId = dto.RoleId,
                                PermissionId = permId
                            };
                            rolePerm.MarkAsCreated(_currentUserService.UserId ?? "System");
                            await repo.AddAsync(rolePerm, cancellationToken);
                        }
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    return ApiResponse<bool>.Success(true, $"{dto.PermissionIds.Count} permission berhasil di-assign");
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk assigning permissions");
                return ApiResponse<bool>.Danger("Gagal bulk assign permission", ex);
            }
        }

        public async Task<ApiResponse<List<RolePermissionDto>>> GetByRoleAsync(string roleId, RolePermissionFilterDto? filter = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _unitOfWork.Repository<RolePermission, int>().Query()
                    .Where(rp => rp.RoleId == roleId)
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .AsQueryable();

                if (filter?.Module != null)
                    query = query.Where(rp => rp.Permission.Module == filter.Module);

                var list = await query.ToListAsync(cancellationToken);

                var dtos = list.Select(rp => new RolePermissionDto
                {
                    Id = rp.Id,
                    RoleId = rp.RoleId,
                    RoleName = rp.Role.Name!,
                    PermissionId = rp.PermissionId,
                    PermissionCode = rp.Permission.Code,
                    PermissionName = rp.Permission.Name,
                    Module = rp.Permission.Module,
                    AssignedAt = rp.CreatedAt,
                    AssignedBy = rp.CreatedBy
                }).ToList();

                return ApiResponse<List<RolePermissionDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions by role");
                return ApiResponse<List<RolePermissionDto>>.Danger("Gagal mengambil data", ex);
            }
        }

        public async Task<ApiResponse<List<RolePermissionDto>>> GetByPermissionAsync(int permissionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var list = await _unitOfWork.Repository<RolePermission, int>().Query()
                    .Where(rp => rp.PermissionId == permissionId)
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .ToListAsync(cancellationToken);

                var dtos = list.Select(rp => new RolePermissionDto
                {
                    Id = rp.Id,
                    RoleId = rp.RoleId,
                    RoleName = rp.Role.Name!,
                    PermissionId = rp.PermissionId,
                    PermissionCode = rp.Permission.Code,
                    PermissionName = rp.Permission.Name,
                    Module = rp.Permission.Module,
                    AssignedAt = rp.CreatedAt,
                    AssignedBy = rp.CreatedBy
                }).ToList();

                return ApiResponse<List<RolePermissionDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles by permission");
                return ApiResponse<List<RolePermissionDto>>.Danger("Gagal mengambil data", ex);
            }
        }

        public async Task<ApiResponse<bool>> HasPermissionAsync(string roleId, string permissionCode, CancellationToken cancellationToken = default)
        {
            var hasPerm = await _unitOfWork.Repository<RolePermission, int>().Query()
                .Include(rp => rp.Permission)
                .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.Code == permissionCode && !rp.Permission.IsDeleted, cancellationToken);

            return ApiResponse<bool>.Success(hasPerm);
        }

        public async Task<ApiResponse<bool>> SyncRolePermissionsAsync(string roleId, List<int> permissionIds, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUserService.IsAdmin() && !_currentUserService.IsSuperAdmin())
                    return ApiResponse<bool>.Forbidden("Hanya Admin yang dapat sync permissions");

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                try
                {
                    var repo = _unitOfWork.Repository<RolePermission, int>();

                    // Get existing
                    var existing = await repo.Query()
                        .Where(rp => rp.RoleId == roleId)
                        .ToListAsync(cancellationToken);

                    // Remove permissions not in new list
                    var toRemove = existing.Where(e => !permissionIds.Contains(e.PermissionId)).ToList();
                    foreach (var item in toRemove)
                    {
                        repo.HardDelete(item);
                    }

                    // Add new permissions
                    var existingIds = existing.Select(e => e.PermissionId).ToList();
                    var toAdd = permissionIds.Where(id => !existingIds.Contains(id)).ToList();

                    foreach (var permId in toAdd)
                    {
                        var rolePerm = new RolePermission
                        {
                            RoleId = roleId,
                            PermissionId = permId
                        };
                        rolePerm.MarkAsCreated(_currentUserService.UserId ?? "System");
                        await repo.AddAsync(rolePerm, cancellationToken);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    return ApiResponse<bool>.Success(true, $"Sync berhasil: {toAdd.Count} ditambahkan, {toRemove.Count} dihapus");
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing role permissions");
                return ApiResponse<bool>.Danger("Gagal sync permission", ex);
            }
        }
    }
}
