// RoleDtos.cs
namespace ApiGMPKlik.DTOs
{
    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public int UserCount { get; set; }
        public int PermissionCount { get; set; }
    }

    public class CreateRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    public class UpdateRoleDto
    {
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class RoleFilterDto
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}

// PermissionDtos.cs
namespace ApiGMPKlik.DTOs
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RoleCount { get; set; }
    }

    public class CreatePermissionDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public int SortOrder { get; set; } = 0;
    }

    public class UpdatePermissionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class PermissionFilterDto
    {
        public string? Search { get; set; }
        public string? Module { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}

// RolePermissionDtos.cs
namespace ApiGMPKlik.DTOs
{
    public class RolePermissionDto
    {
        public int Id { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
    }

    public class AssignPermissionToRoleDto
    {
        public string RoleId { get; set; } = string.Empty;
        public int PermissionId { get; set; }
    }

    public class BulkAssignPermissionDto
    {
        public string RoleId { get; set; } = string.Empty;
        public List<int> PermissionIds { get; set; } = new();
    }

    public class RolePermissionFilterDto
    {
        public string? RoleId { get; set; }
        public string? Module { get; set; }
    }
}

// UserRoleDtos.cs
namespace ApiGMPKlik.DTOs
{
    public class UserRoleDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }

    public class AssignRoleToUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class UserRoleFilterDto
    {
        public string? UserId { get; set; }
        public string? RoleId { get; set; }
        public string? Search { get; set; }
    }
}



