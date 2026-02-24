namespace ApiGMPKlik.Application.Interfaces
{
    /// <summary>
    /// Service untuk mengakses informasi user yang sedang login
    /// Digunakan untuk role-based data filtering
    /// </summary>
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        int? BranchId { get; }
        IReadOnlyList<string> Roles { get; }
        bool IsAuthenticated { get; }

        bool IsInRole(string role);
        bool IsInAnyRole(params string[] roles);
        bool IsSuperAdmin();
        bool IsAdmin();
        bool IsBranchAdmin();
        bool CanAccessAllData();
        bool CanAccessBranchData(int branchId);
        bool CanAccessOwnData(string ownerId);
    }
}
