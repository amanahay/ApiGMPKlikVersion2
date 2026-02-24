// IRoleService.cs
using ApiGMPKlik.DTOs;
using ApiGMPKlik.Shared;

namespace ApiGMPKlik.Interfaces
{
    public interface IRoleService
    {
        Task<ApiResponse<RoleDto>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<ApiResponse<RoleDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RoleDto>>> GetAllAsync(RoleFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<PaginatedList<RoleDto>>> GetPaginatedAsync(RoleFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<RoleDto>> UpdateAsync(string id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> RestoreAsync(string id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ExistsAsync(string name, CancellationToken cancellationToken = default);
        Task<ApiResponse<int>> CountAsync(RoleFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<PermissionDto>>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default);
    }
}

// IPermissionService.cs
namespace ApiGMPKlik.Interfaces
{
    public interface IPermissionService
    {
        Task<ApiResponse<PermissionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<PermissionDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<PermissionDto>>> GetAllAsync(PermissionFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<PaginatedList<PermissionDto>>> GetPaginatedAsync(PermissionFilterDto? filter = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<ApiResponse<PermissionDto>> CreateAsync(CreatePermissionDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<PermissionDto>> UpdateAsync(int id, UpdatePermissionDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> RestoreAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ExistsAsync(string code, CancellationToken cancellationToken = default);
        Task<ApiResponse<int>> CountAsync(PermissionFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<string>>> GetModulesAsync(CancellationToken cancellationToken = default);
    
        Task SyncUserPermissionsToClaimsAsync(string userId, System.Security.Claims.ClaimsIdentity identity);
    }
}

// IRolePermissionService.cs

namespace ApiGMPKlik.Interfaces
{
    public interface IRolePermissionService
    {
        Task<ApiResponse<RolePermissionDto>> AssignPermissionAsync(AssignPermissionToRoleDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> RemovePermissionAsync(string roleId, int permissionId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> BulkAssignAsync(BulkAssignPermissionDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RolePermissionDto>>> GetByRoleAsync(string roleId, RolePermissionFilterDto? filter = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RolePermissionDto>>> GetByPermissionAsync(int permissionId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> HasPermissionAsync(string roleId, string permissionCode, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> SyncRolePermissionsAsync(string roleId, List<int> permissionIds, CancellationToken cancellationToken = default);
    }
}

// IUserRoleService.cs

namespace ApiGMPKlik.Interfaces
{
    public interface IUserRoleService
    {
        Task<ApiResponse<UserRoleDto>> AssignRoleAsync(AssignRoleToUserDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> RemoveRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<UserRoleDto>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<UserRoleDto>>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> IsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> BulkAssignAsync(string userId, List<string> roleNames, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> BulkRemoveAsync(string userId, List<string> roleNames, CancellationToken cancellationToken = default);
    }
}

// IUserClaimService.cs

namespace ApiGMPKlik.Interfaces
{
    public interface IUserClaimService
    {
        Task<ApiResponse<UserClaimDto>> CreateAsync(CreateUserClaimDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<UserClaimDto>>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> UpdateAsync(int id, UpdateUserClaimDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> RemoveClaimAsync(string userId, string claimType, string claimValue, CancellationToken cancellationToken = default);
    }
}

// IRoleClaimService.cs


namespace ApiGMPKlik.Interfaces
{
    public interface IRoleClaimService
    {
        Task<ApiResponse<RoleClaimDto>> CreateAsync(CreateRoleClaimDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<RoleClaimDto>>> GetByRoleAsync(string roleId, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> RemoveClaimAsync(string roleId, string claimType, string claimValue, CancellationToken cancellationToken = default);
    }
}

// IUserLoginService.cs

namespace ApiGMPKlik.Interfaces
{
    public interface IUserLoginService
    {
        Task<ApiResponse<UserLoginDto>> CreateAsync(CreateUserLoginDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> DeleteAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<UserLoginDto>>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<ApiResponse<UserLoginDto?>> FindAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default);
    }
}