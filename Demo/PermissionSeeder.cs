using ApiGMPKlik.Models;
using ApiGMPKlik.Models.Entities;
using Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiGMPKlik.Demo
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)  // <-- UBAH KE ApplicationRole
        {
            // Seed Permissions
            var permissions = new[]
            {
                new Permission { Code = "WEATHER_READ", Name = "Read Weather Data", Module = "Weather", SortOrder = 1 },
                new Permission { Code = "WEATHER_CREATE", Name = "Create Weather Forecast", Module = "Weather", SortOrder = 2 },
                new Permission { Code = "WEATHER_UPDATE", Name = "Update Weather Data", Module = "Weather", SortOrder = 3 },
                new Permission { Code = "WEATHER_DELETE", Name = "Delete Weather Data", Module = "Weather", SortOrder = 4 },
                new Permission { Code = "USER_CREATE", Name = "Create User", Module = "User", SortOrder = 5 },
                new Permission { Code = "USER_READ", Name = "Read User", Module = "User", SortOrder = 6 },
                new Permission { Code = "USER_UPDATE", Name = "Update User", Module = "User", SortOrder = 7 },
                new Permission { Code = "USER_DELETE", Name = "Delete User", Module = "User", SortOrder = 8 },
                new Permission { Code = "APIKEY_MANAGE", Name = "Manage API Keys", Module = "System", SortOrder = 9 }
            };

            foreach (var perm in permissions)
            {
                if (!await context.Permissions.AnyAsync(p => p.Code == perm.Code))
                {
                    perm.CreatedAt = DateTime.UtcNow;
                    perm.CreatedBy = "System";
                    await context.Permissions.AddAsync(perm);
                }
            }
            await context.SaveChangesAsync();

            // Seed Roles dengan ApplicationRole
            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
            {
                adminRole = new ApplicationRole  // <-- UBAH KE ApplicationRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                await roleManager.CreateAsync(adminRole);
            }

            var managerRole = await roleManager.FindByNameAsync("Manager");
            if (managerRole == null)
            {
                managerRole = new ApplicationRole  // <-- UBAH KE ApplicationRole
                {
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                await roleManager.CreateAsync(managerRole);
            }

            var direkturRole = await roleManager.FindByNameAsync("Direktur");
            if (direkturRole == null)
            {
                direkturRole = new ApplicationRole  // <-- UBAH KE ApplicationRole
                {
                    Name = "Direktur",
                    NormalizedName = "DIREKTUR",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                await roleManager.CreateAsync(direkturRole);
            }

            var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
            if (superAdminRole == null)
            {
                superAdminRole = new ApplicationRole  // <-- UBAH KE ApplicationRole
                {
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                await roleManager.CreateAsync(superAdminRole);
            }

            var tamuRole = await roleManager.FindByNameAsync("Tamu");
            if (tamuRole == null)
            {
                tamuRole = new ApplicationRole  // <-- UBAH KE ApplicationRole
                {
                    Name = "Tamu",
                    NormalizedName = "TAMU",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                await roleManager.CreateAsync(tamuRole);
            }

            // Assign all permissions to Admin 
            var allPermissions = await context.Permissions.ToListAsync();
            foreach (var perm in allPermissions)
            {
                if (!await context.RolePermissions.AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == perm.Id))
                {
                    await context.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = perm.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    });
                }
            }

            // Assign Weather permissions to Manager
            var weatherPerms = allPermissions.Where(p => p.Module == "Weather").ToList();
            foreach (var perm in weatherPerms)
            {
                if (!await context.RolePermissions.AnyAsync(rp => rp.RoleId == managerRole.Id && rp.PermissionId == perm.Id))
                {
                    await context.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = managerRole.Id,
                        PermissionId = perm.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    });
                }
            }

            // Assign ALL permissions to SuperAdmin
            foreach (var perm in allPermissions)
            {
                if (!await context.RolePermissions.AnyAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == perm.Id))
                {
                    await context.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = superAdminRole.Id,
                        PermissionId = perm.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    });
                }
            }

            await context.SaveChangesAsync();

            // Seed SuperAdmin User
            var superAdminEmail = "superadmin@gmpklik.local";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = "superadmin",
                    Email = superAdminEmail,
                    FullName = "System Super Administrator",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    BranchId = null // SuperAdmin tidak terikat cabang
                };

                var result = await userManager.CreateAsync(superAdminUser, "@superadmin2626");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                }
            }
        }
    }
}