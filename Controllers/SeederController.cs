using Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiGMPKlik.Models;
using ApiGMPKlik.Models.Entities;

namespace ApiGMPKlik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeederController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public SeederController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpPost("execute-all")]
        public async Task<IActionResult> ExecuteAll()
        {
            try
            {
                var result = new List<string>();

                result.AddRange(await SeedRolesAsync());
                result.AddRange(await SeedBranchesAsync());
                result.AddRange(await SeedUsersAsync());
                result.AddRange(await SeedUserProfilesAsync());
                result.AddRange(await SeedUserSecurityAsync());
                result.AddRange(await SeedReferralTreesAsync()); // TAMBAHAN BARU

                return Ok(new
                {
                    message = "Seeding completed successfully",
                    summary = new
                    {
                        roles = 9,
                        branches = 4,
                        users = 7,
                        userProfiles = 7,
                        userSecurity = 7,
                        referralTrees = 6
                    },
                    details = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }

        private async Task<List<string>> SeedRolesAsync()
        {
            var logs = new List<string>();
            var roles = new List<ApplicationRole>
            {
                new ApplicationRole
                {
                    Id = "8cbcec6f-a83c-49be-b211-d3e1cec44d98",
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    Description = "Super Administrator dengan akses penuh",
                    SortOrder = 1,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = "admin-role-id",
                    Name = "TenantAdmin",
                    NormalizedName = "TENANTADMIN",
                    Description = "Tenant Administrator",
                    SortOrder = 2,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = "PTG-ADMIN-PXG2yrRP",
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR",
                    Description = "Administrator",
                    SortOrder = 3,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = "PTG-AGEN-oY3QQcCI",
                    Name = "Agen",
                    NormalizedName = "AGEN",
                    Description = "Agen",
                    SortOrder = 4,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = "PTG-DIREKT-1xApAVma",
                    Name = "Direktur",
                    NormalizedName = "DIREKTUR",
                    Description = "Direktur",
                    SortOrder = 5,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = "PTG-MANAGE-AjJt6fXR",
                    Name = "Management",
                    NormalizedName = "MANAGEMENT",
                    Description = "Management",
                    SortOrder = 6,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = "readonly-role-id",
                    Name = "ReadOnly",
                    NormalizedName = "READONLY",
                    Description = "Read Only Access",
                    SortOrder = 7,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = "SYST-KOMISA-f89CC0zi",
                    Name = "Komisaris",
                    NormalizedName = "KOMISARIS",
                    Description = "Komisaris",
                    SortOrder = 8,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = "user-role-id",
                    Name = "TenantUser",
                    NormalizedName = "TENANTUSER",
                    Description = "Tenant User",
                    SortOrder = 9,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            };

            foreach (var role in roles)
            {
                var existing = await _roleManager.FindByIdAsync(role.Id);
                if (existing == null)
                {
                    role.MarkAsCreated("System");
                    await _roleManager.CreateAsync(role);
                    logs.Add($"✅ Created role: {role.Name}");
                }
                else
                {
                    logs.Add($"⏭️ Skipped role: {role.Name} (already exists)");
                }
            }

            return logs;
        }

        private async Task<List<string>> SeedBranchesAsync()
        {
            var logs = new List<string>();

            var branchesData = new[]
            {
                new {
                    Name = "Cabang Bekasi",
                    Code = "01",
                    Address = "Bekasi",
                    City = "Bekasi",
                    Province = "Jawa Barat",
                    IsMainBranch = true,
                    SortOrder = 1
                },
                new {
                    Name = "Cabang Bandung",
                    Code = "02",
                    Address = "Bandung",
                    City = "Bandung",
                    Province = "Jawa Barat",
                    IsMainBranch = false,
                    SortOrder = 2
                },
                new {
                    Name = "Cabang Jakarta",
                    Code = "03",
                    Address = "Jakarta",
                    City = "Jakarta",
                    Province = "DKI Jakarta",
                    IsMainBranch = false,
                    SortOrder = 3
                },
                new {
                    Name = "Cabang Depok",
                    Code = "04",
                    Address = "Depok",
                    City = "Depok",
                    Province = "Jawa Barat",
                    IsMainBranch = false,
                    SortOrder = 4
                }
            };

            foreach (var data in branchesData)
            {
                var existing = await _context.Branches.FirstOrDefaultAsync(b => b.Code == data.Code);
                if (existing == null)
                {
                    var branch = new Branch
                    {
                        Name = data.Name,
                        Code = data.Code,
                        Address = data.Address,
                        City = data.City,
                        Province = data.Province,
                        IsMainBranch = data.IsMainBranch,
                        IsActive = true,
                        Phone = null,
                        Email = null,
                        PostalCode = null,
                        Latitude = null,
                        Longitude = null
                    };
                    branch.MarkAsCreated("system");

                    _context.Branches.Add(branch);
                    logs.Add($"✅ Created branch: {data.Name} (Code: {data.Code})");
                }
                else
                {
                    logs.Add($"⏭️ Skipped branch: {data.Name} (already exists)");
                }
            }

            await _context.SaveChangesAsync();
            return logs;
        }

        private async Task<List<string>> SeedUsersAsync()
        {
            var logs = new List<string>();
            var defaultPassword = "Test12345";

            var branches = await _context.Branches.ToListAsync();
            var branchMap = branches.Where(b => !string.IsNullOrEmpty(b.Code)).ToDictionary(b => b.Code!, b => b.Id);

            if (branches.Count < 4)
            {
                logs.Add("⚠️ Warning: Not all branches available. Run seeding again.");
                return logs;
            }

            var usersData = new List<(ApplicationUser User, string[] Roles, string BranchCode)>
            {
                (
                    new ApplicationUser
                    {
                        Id = "user-superadmin-001",
                        UserName = "superadmin",
                        Email = "superadmin@example.com",
                        NormalizedUserName = "SUPERADMIN",
                        NormalizedEmail = "SUPERADMIN@EXAMPLE.COM",
                        FullName = "Super Administrator",
                        EmailConfirmed = true,
                        PhoneNumber = "081234567001",
                        PhoneNumberConfirmed = true,
                        IsActive = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new[] { "SuperAdmin" },
                    "01"
                ),
                (
                    new ApplicationUser
                    {
                        Id = "user-direktur-001",
                        UserName = "direktur",
                        Email = "direktur@example.com",
                        NormalizedUserName = "DIREKTUR",
                        NormalizedEmail = "DIREKTUR@EXAMPLE.COM",
                        FullName = "Budi Santoso (Direktur)",
                        EmailConfirmed = true,
                        PhoneNumber = "081234567002",
                        PhoneNumberConfirmed = true,
                        IsActive = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new[] { "Direktur", "Administrator" },
                    "03"
                ),
                (
                    new ApplicationUser
                    {
                        Id = "user-komisaris-001",
                        UserName = "komisaris",
                        Email = "komisaris@example.com",
                        NormalizedUserName = "KOMISARIS",
                        NormalizedEmail = "KOMISARIS@EXAMPLE.COM",
                        FullName = "Ahmad Hidayat (Komisaris)",
                        EmailConfirmed = true,
                        PhoneNumber = "081234567003",
                        PhoneNumberConfirmed = true,
                        IsActive = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new[] { "Komisaris", "Management" },
                    "01"
                ),
                (
                    new ApplicationUser
                    {
                        Id = "user-manager-001",
                        UserName = "manager",
                        Email = "manager@example.com",
                        NormalizedUserName = "MANAGER",
                        NormalizedEmail = "MANAGER@EXAMPLE.COM",
                        FullName = "Siti Aminah (Manager)",
                        EmailConfirmed = true,
                        PhoneNumber = "081234567004",
                        PhoneNumberConfirmed = true,
                        IsActive = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new[] { "Management", "Agen" },
                    "02"
                ),
                (
                    new ApplicationUser
                    {
                        Id = "user-admin-001",
                        UserName = "admin",
                        Email = "admin@example.com",
                        NormalizedUserName = "ADMIN",
                        NormalizedEmail = "ADMIN@EXAMPLE.COM",
                        FullName = "Dewi Kusuma (Admin & Agen)",
                        EmailConfirmed = true,
                        PhoneNumber = "081234567005",
                        PhoneNumberConfirmed = true,
                        IsActive = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new[] { "Administrator", "Agen" },
                    "04"
                ),
                (
                    new ApplicationUser
                    {
                        Id = "user-agen-001",
                        UserName = "agen1",
                        Email = "agen1@example.com",
                        NormalizedUserName = "AGEN1",
                        NormalizedEmail = "AGEN1@EXAMPLE.COM",
                        FullName = "Rudi Hartono (Agen)",
                        EmailConfirmed = true,
                        PhoneNumber = "081234567006",
                        PhoneNumberConfirmed = true,
                        IsActive = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new[] { "Agen" },
                    "02"
                ),
                (
                    new ApplicationUser
                    {
                        Id = "user-tenant-001",
                        UserName = "tenantadmin",
                        Email = "tenant@example.com",
                        NormalizedUserName = "TENANTADMIN",
                        NormalizedEmail = "TENANT@EXAMPLE.COM",
                        FullName = "Tenant Administrator",
                        EmailConfirmed = true,
                        PhoneNumber = "081234567007",
                        PhoneNumberConfirmed = true,
                        IsActive = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new[] { "TenantAdmin" },
                    "03"
                )
            };

            foreach (var (user, roles, branchCode) in usersData)
            {
                var existing = await _userManager.FindByEmailAsync(user.Email!);
                if (existing == null)
                {
                    if (branchMap.ContainsKey(branchCode))
                    {
                        user.BranchId = branchMap[branchCode];
                    }

                    user.MarkAsCreated("System");
                    var result = await _userManager.CreateAsync(user, defaultPassword);

                    if (result.Succeeded)
                    {
                        var branchName = branches.FirstOrDefault(b => b.Code == branchCode)?.Name ?? "Unknown";
                        logs.Add($"✅ Created user: {user.UserName} ({user.Email}) -> Branch: {branchName}");

                        foreach (var roleName in roles)
                        {
                            var roleExists = await _roleManager.RoleExistsAsync(roleName);
                            if (roleExists)
                            {
                                await _userManager.AddToRoleAsync(user, roleName);
                                logs.Add($"   └─ Assigned role: {roleName}");
                            }
                            else
                            {
                                logs.Add($"   ⚠️ Role not found: {roleName}");
                            }
                        }
                    }
                    else
                    {
                        logs.Add($"❌ Failed to create {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logs.Add($"⏭️ Skipped user: {user.UserName} (already exists)");
                }
            }

            return logs;
        }

        private async Task<List<string>> SeedUserProfilesAsync()
        {
            var logs = new List<string>();
            var userIds = new[]
            {
                "user-superadmin-001", "user-direktur-001", "user-komisaris-001",
                "user-manager-001", "user-admin-001", "user-agen-001", "user-tenant-001"
            };

            foreach (var userId in userIds)
            {
                var exists = await _context.UserProfiles.AnyAsync(p => p.UserId == userId);
                if (!exists)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var profile = new UserProfile
                        {
                            UserId = userId,
                            Gender = user.UserName!.Contains("dewi") || user.UserName.Contains("siti") ? "Female" : "Male",
                            Address = $"Alamat {user.FullName}",
                            Language = "id",
                            NewsletterSubscribed = false,
                            Balance = 0,
                            Commission = 0
                        };
                        profile.MarkAsCreated("System");

                        _context.UserProfiles.Add(profile);
                        logs.Add($"✅ Created profile for: {user.UserName}");
                    }
                }
                else
                {
                    logs.Add($"⏭️ Profile exists for: {userId}");
                }
            }

            await _context.SaveChangesAsync();
            return logs;
        }

        private async Task<List<string>> SeedUserSecurityAsync()
        {
            var logs = new List<string>();
            var userIds = new[]
            {
                "user-superadmin-001", "user-direktur-001", "user-komisaris-001",
                "user-manager-001", "user-admin-001", "user-agen-001", "user-tenant-001"
            };

            foreach (var userId in userIds)
            {
                var exists = await _context.UserSecurities.AnyAsync(s => s.UserId == userId);
                if (!exists)
                {
                    var security = new UserSecurity
                    {
                        UserId = userId,
                        FailedLoginAttempts = 0,
                        RequirePasswordChange = false
                    };

                    _context.UserSecurities.Add(security);
                    logs.Add($"✅ Created security record for: {userId}");
                }
                else
                {
                    logs.Add($"⏭️ Security record exists for: {userId}");
                }
            }

            await _context.SaveChangesAsync();
            return logs;
        }

        // ==========================================
        // METHOD BARU: SEED REFERRAL TREE
        // ==========================================
        private async Task<List<string>> SeedReferralTreesAsync()
        {
            var logs = new List<string>();

            // Struktur Hierarki Referral:
            // Level 0: superadmin (Root Utama)
            //   ├── Level 1: direktur (Parent: superadmin, Commission: 10%)
            //   │   └── Level 2: manager (Parent: direktur, Commission: 5%)
            //   │       └── Level 3: agen1 (Parent: manager, Commission: 2.5%)
            //   ├── Level 1: komisaris (Parent: superadmin, Commission: 10%)
            //   │   └── Level 2: admin (Parent: komisaris, Commission: 5%)
            //   └── Level 1: tenantadmin (Parent: superadmin, Commission: 10%)

            var referralData = new[]
            {
                // Level 1 - Direct referrals from superadmin
                new {
                    RootUserId = "user-superadmin-001",
                    ReferredUserId = "user-direktur-001",
                    ParentUserId = "user-superadmin-001",
                    Level = 1,
                    CommissionPercent = 10.0m
                },
                new {
                    RootUserId = "user-superadmin-001",
                    ReferredUserId = "user-komisaris-001",
                    ParentUserId = "user-superadmin-001",
                    Level = 1,
                    CommissionPercent = 10.0m
                },
                new {
                    RootUserId = "user-superadmin-001",
                    ReferredUserId = "user-tenant-001",
                    ParentUserId = "user-superadmin-001",
                    Level = 1,
                    CommissionPercent = 10.0m
                },
                
                // Level 2 - Referrals from Level 1 users
                new {
                    RootUserId = "user-superadmin-001",
                    ReferredUserId = "user-manager-001",
                    ParentUserId = "user-direktur-001", // direktur yang merefer manager
                    Level = 2,
                    CommissionPercent = 5.0m
                },
                new {
                    RootUserId = "user-superadmin-001",
                    ReferredUserId = "user-admin-001",
                    ParentUserId = "user-komisaris-001", // komisaris yang merefer admin
                    Level = 2,
                    CommissionPercent = 5.0m
                },
                
                // Level 3 - Referrals from Level 2 users
                new {
                    RootUserId = "user-superadmin-001",
                    ReferredUserId = "user-agen-001",
                    ParentUserId = "user-manager-001", // manager yang merefer agen1
                    Level = 3,
                    CommissionPercent = 2.5m
                }
            };

            foreach (var data in referralData)
            {
                // Validasi: Cek apakah sudah ada (berdasarkan RootUserId + ReferredUserId)
                var existing = await _context.ReferralTrees
                    .FirstOrDefaultAsync(r =>
                        r.RootUserId == data.RootUserId &&
                        r.ReferredUserId == data.ReferredUserId);

                if (existing == null)
                {
                    // Validasi: Pastikan user ada
                    var rootUser = await _userManager.FindByIdAsync(data.RootUserId);
                    var referredUser = await _userManager.FindByIdAsync(data.ReferredUserId);
                    var parentUser = await _userManager.FindByIdAsync(data.ParentUserId);

                    if (rootUser == null || referredUser == null || parentUser == null)
                    {
                        logs.Add($"❌ Skipped: User not found for referral {data.RootUserId} -> {data.ReferredUserId}");
                        continue;
                    }

                    // Validasi: Tidak boleh self-referral
                    if (data.RootUserId == data.ReferredUserId)
                    {
                        logs.Add($"❌ Skipped: Self-referral not allowed for {data.ReferredUserId}");
                        continue;
                    }

                    // Validasi: Level harus 1-3
                    if (data.Level < 1 || data.Level > 3)
                    {
                        logs.Add($"❌ Skipped: Invalid level {data.Level} for {data.ReferredUserId}");
                        continue;
                    }

                    var referral = new ReferralTree
                    {
                        RootUserId = data.RootUserId,
                        ReferredUserId = data.ReferredUserId,
                        ParentUserId = data.ParentUserId,
                        Level = data.Level,
                        CommissionPercent = data.CommissionPercent,
                        IsActive = true,
                        IsDeleted = false
                    };

                    referral.MarkAsCreated("System");
                    _context.ReferralTrees.Add(referral);

                    logs.Add($"✅ Created referral: {referredUser.UserName} (Level {data.Level}, Parent: {parentUser.UserName}, Commission: {data.CommissionPercent}%)");
                }
                else
                {
                    var referredUser = await _userManager.FindByIdAsync(data.ReferredUserId);
                    logs.Add($"⏭️ Skipped referral: {referredUser?.UserName ?? data.ReferredUserId} (already exists)");
                }
            }

            await _context.SaveChangesAsync();
            return logs;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var rolesCount = await _roleManager.Roles.CountAsync();
            var branchesCount = await _context.Branches.CountAsync();
            var usersCount = await _userManager.Users.CountAsync();
            var profilesCount = await _context.UserProfiles.CountAsync();
            var securityCount = await _context.UserSecurities.CountAsync();
            var referralCount = await _context.ReferralTrees.CountAsync();

            return Ok(new
            {
                roles = new { total = rolesCount, target = 9 },
                branches = new { total = branchesCount, target = 4 },
                users = new { total = usersCount, target = 7 },
                userProfiles = new { total = profilesCount, target = 7 },
                userSecurity = new { total = securityCount, target = 7 },
                referralTrees = new { total = referralCount, target = 6 },
                fullySeeded = rolesCount >= 9 && branchesCount >= 4 && usersCount >= 7 && referralCount >= 6
            });
        }

        [HttpDelete("reset-referrals")]
        public async Task<IActionResult> ResetReferrals()
        {
            try
            {
                var referrals = await _context.ReferralTrees.ToListAsync();
                _context.ReferralTrees.RemoveRange(referrals);
                await _context.SaveChangesAsync();
                return Ok(new { message = "All referral trees deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("reset-users")]
        public async Task<IActionResult> ResetUsers()
        {
            try
            {
                // Hapus ReferralTrees dulu (FK constraint)
                var referrals = await _context.ReferralTrees.ToListAsync();
                _context.ReferralTrees.RemoveRange(referrals);

                var profiles = await _context.UserProfiles.ToListAsync();
                _context.UserProfiles.RemoveRange(profiles);

                var securities = await _context.UserSecurities.ToListAsync();
                _context.UserSecurities.RemoveRange(securities);

                var users = await _userManager.Users.ToListAsync();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, roles);
                    }
                }

                foreach (var user in users)
                {
                    await _userManager.DeleteAsync(user);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "All users and related data deleted. Branches kept intact." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("reset-all")]
        public async Task<IActionResult> ResetAll()
        {
            try
            {
                // Hapus semua data termasuk branches
                var referrals = await _context.ReferralTrees.ToListAsync();
                _context.ReferralTrees.RemoveRange(referrals);

                var profiles = await _context.UserProfiles.ToListAsync();
                _context.UserProfiles.RemoveRange(profiles);

                var securities = await _context.UserSecurities.ToListAsync();
                _context.UserSecurities.RemoveRange(securities);

                var users = await _userManager.Users.ToListAsync();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, roles);
                    }
                }

                foreach (var user in users)
                {
                    await _userManager.DeleteAsync(user);
                }

                var branches = await _context.Branches.ToListAsync();
                _context.Branches.RemoveRange(branches);

                await _context.SaveChangesAsync();
                return Ok(new { message = "All data deleted (users, profiles, securities, branches, referrals)" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}