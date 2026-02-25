using ApiGMPKlik.Application.Interfaces;
using ApiGMPKlik.Demo;
using ApiGMPKlik.Infrastructure;
using ApiGMPKlik.Infrastructure.Address;
using ApiGMPKlik.Interfaces;
using ApiGMPKlik.Interfaces.DataPrice;
using ApiGMPKlik.Interfaces.Repositories;
using ApiGMPKlik.Models;
using ApiGMPKlik.Services;
using ApiGMPKlik.Services.Address;
using ApiGMPKlik.Services.DataPrice;
using ApiGMPKlik.Shared;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors.Security;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 0. STATIC AUTH SETTINGS (PIN Configuration)
// ============================================
builder.Services.Configure<StaticAuthSettings>(
    builder.Configuration.GetSection("StaticAuth"));

// ============================================
// 1. CONTROLLERS & JSON CONFIGURATION
// ============================================
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ============================================
// 2. AUTHENTICATION CONFIGURATION
// ============================================
// FIX KRITIS: Semua authentication schemes harus didaftarkan dalam satu AddAuthentication()
// untuk menghindari satu scheme menimpa yang lain.

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"]!;
var jwtIssuer = jwtSettings["Issuer"]!;
var jwtAudience = jwtSettings["Audience"]!;

// DEBUG: Log konfigurasi JWT
var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var startupLogger = loggerFactory.CreateLogger<Program>();
startupLogger.LogInformation("=== JWT Configuration ===");
startupLogger.LogInformation("Issuer: {Issuer}", jwtIssuer);
startupLogger.LogInformation("Audience: {Audience}", jwtAudience);
startupLogger.LogInformation("Key Length: {KeyLength}", jwtKey.Length);

builder.Services.AddAuthentication(options =>
{
    // Default scheme untuk API endpoints
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
// JWT Bearer Authentication - PRIMARY
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(5), // Beri toleransi 5 menit untuk clock skew
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            logger.LogInformation("=== JWT OnMessageReceived ===");
            logger.LogInformation("Path: {Path}", context.Request.Path);
            logger.LogInformation("Auth Header Present: {HasAuth}", !string.IsNullOrEmpty(authHeader));

            if (!string.IsNullOrEmpty(authHeader))
            {
                logger.LogInformation("Auth Header: {Header}",
                    authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader);
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            logger.LogInformation("=== JWT OnTokenValidated ===");
            logger.LogInformation("Token validated successfully!");
            logger.LogInformation("User: {User}", context.Principal?.Identity?.Name ?? "Unknown");

            var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            if (claims != null)
            {
                logger.LogInformation("Claims: {Claims}", string.Join(", ", claims));
            }

            // Sync permissions jika ada PermissionService
            try
            {
                var permissionService = context.HttpContext.RequestServices
                    .GetService<IPermissionService>();

                if (permissionService != null)
                {
                    var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId != null && context.Principal?.Identity is ClaimsIdentity identity)
                    {
                        await permissionService.SyncUserPermissionsToClaimsAsync(userId, identity);
                        logger.LogInformation("Permissions synced for user: {UserId}", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                // Jangan biarkan error di sini menggagalkan autentikasi
                logger.LogWarning(ex, "Failed to sync permissions, but auth will continue");
            }
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            logger.LogError("=== JWT OnAuthenticationFailed ===");
            logger.LogError("Exception: {Message}", context.Exception.Message);
            logger.LogError("Exception Type: {Type}", context.Exception.GetType().Name);

            if (context.Exception.InnerException != null)
            {
                logger.LogError("Inner Exception: {Message}", context.Exception.InnerException.Message);
            }

            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            logger.LogWarning("=== JWT OnChallenge ===");
            logger.LogWarning("Authenticate Failure: {Failure}", context.AuthenticateFailure?.Message ?? "None");

            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var response = ApiResponse<object>.Unauthorized("Token tidak valid atau tidak ditemukan. Silakan login terlebih dahulu.");
            await context.Response.WriteAsync(response.ToJson());
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var response = ApiResponse<object>.Forbidden("Anda tidak memiliki izin.");
            await context.Response.WriteAsync(response.ToJson());
        }
    };
})
// Cookie Authentication (untuk Swagger UI protection)
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/login.html";
    options.LogoutPath = "/api/auth/logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(
        builder.Configuration.GetValue<int>("StaticAuth:SessionTimeoutMinutes", 60));
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = builder.Configuration["StaticAuth:CookieName"] ?? "AuthSession";
})
// API Key Authentication - SECONDARY (untuk service-to-service)
.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
    ApiKeyDefaults.AuthenticationScheme,
    options => { })
// Google Authentication
.AddGoogle(options =>
{
    var googleAuth = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleAuth["ClientId"]!;
    options.ClientSecret = googleAuth["ClientSecret"]!;
    options.CallbackPath = googleAuth["CallbackPath"] ?? "/signin-google";
    options.SaveTokens = true;
    options.Scope.Add("profile");
    options.Scope.Add("email");
});

// ============================================
// 3. API VERSIONING
// ============================================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ============================================
// 4. DATABASE CONTEXTS (Dual Database)
// ============================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServerConnection"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("ApiGMPKlik");
            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sqlOptions.CommandTimeout(60);
        });
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging().EnableDetailedErrors();
});

builder.Services.AddDbContext<SecondaryDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("SQLiteConnection"),
        sqliteOptions =>
        {
            sqliteOptions.MigrationsAssembly("ApiGMPKlik");
        });

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// ============================================
// 5. IDENTITY CONFIGURATION
// ============================================
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
//{
//    options.Password.RequireDigit = true;
//    options.Password.RequireLowercase = false;
//    options.Password.RequireUppercase = false;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequiredLength = 6;
//    options.Password.RequiredUniqueChars = 1;

//    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
//    options.Lockout.MaxFailedAccessAttempts = 5;
//    options.Lockout.AllowedForNewUsers = true;

//    options.User.RequireUniqueEmail = true;
//    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

//    options.SignIn.RequireConfirmedEmail = false;
//    options.SignIn.RequireConfirmedPhoneNumber = false;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddRoles<ApplicationRole>()                          // ← Tambah support Roles
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()                                // ← Tambah SignInManager
.AddDefaultTokenProviders();
// ============================================
// 6. AUTHORIZATION POLICIES
// ============================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("RequireManager", policy => policy.RequireRole("Manager", "Admin", "SuperAdmin"));
    options.AddPolicy("CanViewReports", policy => policy.RequireClaim("Permission", "ViewReports"));
    options.AddPolicy("SwaggerAccess", policy => policy.RequireAuthenticatedUser());
});

// ============================================
// 7. CLEAN ARCHITECTURE SERVICES
// ============================================
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// ============================================
// 8. HANGFIRE
// ============================================
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("SqlServerConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            SchemaName = "Hangfire"
        }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = builder.Configuration.GetValue<int>("HangfireSettings:WorkerCount", 2);
    options.Queues = builder.Configuration.GetSection("HangfireSettings:Queues").Get<string[]>() ?? new[] { "default" };
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
});

// ============================================
// 9. NSWAG 14.x CONFIGURATION
// ============================================
#pragma warning disable ASP0000
var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
#pragma warning restore ASP0000

foreach (var description in provider.ApiVersionDescriptions)
{
    builder.Services.AddOpenApiDocument(config =>
    {
        config.DocumentName = description.GroupName;
        config.Title = $"{builder.Configuration["ApiSettings:Title"]} {description.ApiVersion}";
        config.Version = description.ApiVersion.ToString();
        config.Description = description.IsDeprecated
            ? "API version deprecated."
            : builder.Configuration["ApiSettings:Description"];

        // JWT Security - Http Bearer
        config.AddSecurity("Bearer", new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "Authorization",
            In = OpenApiSecurityApiKeyLocation.Header,
            Description = "Masukkan token dengan format: Bearer {token}"
        });

        // API Key Security
        config.AddSecurity("ApiKey", new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = "X-API-Key",
            In = OpenApiSecurityApiKeyLocation.Header,
            Description = "API Key authentication. Example: \"gmp_xxxx.yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy\""
        });

        config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
        config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("ApiKey"));
        config.OperationProcessors.Add(new RemoveVersionParameterProcessor());
    });
}

builder.Services.AddHttpContextAccessor();

// ============================================
// 10. REPOSITORY & UNIT OF WORK
// ============================================
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISecondaryUnitOfWork, SecondaryUnitOfWork>();

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IUserClaimService, UserClaimService>();
builder.Services.AddScoped<IRoleClaimService, RoleClaimService>();
builder.Services.AddScoped<IUserLoginService, UserLoginService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IUserSecurityService, UserSecurityService>();
builder.Services.AddScoped<IReferralTreeService, ReferralTreeService>();
builder.Services.AddScoped<IBranchService, BranchService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Address Services Registration
builder.Services.AddScoped<IWilayahProvinsiService, WilayahProvinsiService>();
builder.Services.AddScoped<IWilayahKotaKabService, WilayahKotaKabService>();
builder.Services.AddScoped<IWilayahKecamatanService, WilayahKecamatanService>();
builder.Services.AddScoped<IWilayahKelurahanDesaService, WilayahKelurahanDesaService>();
builder.Services.AddScoped<IWilayahDusunService, WilayahDusunService>();
builder.Services.AddScoped<IWilayahRwService, WilayahRwService>();
builder.Services.AddScoped<IWilayahRtService, WilayahRtService>();
builder.Services.AddScoped<IAddressSearchService, AddressSearchService>();

builder.Services.AddScoped<IDataPriceRangeService, DataPriceRangeService>();

builder.Services.AddMemoryCache();
builder.Services.AddPermissionPolicies();

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE - ORDER IS CRITICAL!
// ============================================
// 1. Default Files & Static Files (pertama)
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = false,
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".html"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
    }
});

// 2. Swagger UI Protection (sebelum routing)
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

    if (path.StartsWith("/swag"))
    {
        var authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!authResult.Succeeded)
        {
            context.Response.Cookies.Append("ReturnUrl", context.Request.Path, new CookieOptions
            {
                HttpOnly = true,
                Secure = builder.Environment.IsDevelopment() ? false : true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(5)
            });

            context.Response.Redirect("/login.html");
            return;
        }
    }

    await next();
});

// 3. Routing
app.UseRouting();

// 4. CORS (sebelum auth)
app.UseCors("AllowAll");

// 5. HTTPS Redirection
app.UseHttpsRedirection();

// 6. Response Formatting Middleware (sebelum auth)
app.UseMiddleware<ApiResponseFormattingMiddleware>();

// 7. Authentication & Authorization (setelah routing, sebelum endpoints)
app.UseAuthentication();
app.UseAuthorization();

// 8. Development Middleware
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/swag";
        settings.DocumentTitle = "Identity Membership API Documentation";
        settings.DocExpansion = "none";
        settings.OperationsSorter = "method";
        settings.TagsSorter = "alpha";
        settings.DefaultModelsExpandDepth = 1;
        settings.DefaultModelExpandDepth = 1;
    });

    using (var scope = app.Services.CreateScope())
    {
        //var sqlContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sqliteContext = scope.ServiceProvider.GetRequiredService<SecondaryDbContext>();
        //sqlContext.Database.Migrate();
        sqliteContext.Database.Migrate();
    }
}

// 9. Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "GMPKlik Background Jobs",
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
    IgnoreAntiforgeryToken = true
});

// 10. Endpoints (terakhir)
app.MapControllers();
MapStaticAuthEndpoints(app);

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0"
}));

//// Seeding example data
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var context = services.GetRequiredService<ApplicationDbContext>();
//        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
//        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

//        await PermissionSeeder.SeedAsync(context, userManager, roleManager);

//        var apiKeyService = services.GetRequiredService<IApiKeyService>();
//        if (!context.ApiClients.Any())
//        {
//            await apiKeyService.GenerateApiKeyAsync(
//                "Weather Demo Client",
//                null,
//                DateTime.UtcNow.AddYears(1),
//                new List<string> { "WEATHER_READ" });
//        }
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "Error saat seeding data");
//    }
//}

app.Run();

// ============================================
// STATIC AUTH ENDPOINTS (PIN-Based)
// ============================================
void MapStaticAuthEndpoints(WebApplication app)
{
    app.MapPost("/api/auth/login-pin", async (
        LoginPinRequest request,
        IConfiguration config,
        HttpContext context) =>
    {
        var settings = config.GetSection("StaticAuth").Get<StaticAuthSettings>();

        if (settings == null || string.IsNullOrEmpty(settings.Pin))
        {
            return Results.Problem("PIN not configured", statusCode: 500);
        }

        if (request.Pin != settings.Pin)
        {
            return Results.Unauthorized();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Admin"),
            new Claim(ClaimTypes.Role, "SwaggerViewer"),
            new Claim("AccessLevel", "Full"),
            new Claim(ClaimTypes.NameIdentifier, "static-admin")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(settings.SessionTimeoutMinutes),
            RedirectUri = "/swag"
        };

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Results.Ok(new
        {
            Success = true,
            Message = "Login successful",
            RedirectUrl = "/swag",
            ExpiresAt = authProperties.ExpiresUtc
        });
    }).AllowAnonymous();

    app.MapPost("/api/auth/logout", async (HttpContext context) =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok(new { Success = true, Message = "Logged out successfully" });
    });

    app.MapGet("/api/auth/status", (HttpContext context) =>
    {
        var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
        return Results.Ok(new
        {
            IsAuthenticated = isAuthenticated,
            UserName = context.User?.Identity?.Name,
            Claims = context.User?.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    });
}

// ============================================
// SUPPORTING CLASSES
// ============================================
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}

public class StaticAuthSettings
{
    public string Pin { get; set; } = "123456";
    public string CookieName { get; set; } = "AuthSession";
    public int SessionTimeoutMinutes { get; set; } = 60;
    public int MaxFailedAttempts { get; set; } = 5;
}

public class LoginPinRequest
{
    public string Pin { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}

// ============================================
// CUSTOM RESPONSE MIDDLEWARE
// ============================================
public class ApiResponseFormattingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string[] _excludedPaths = { "/swag", "/hangfire", "/api/auth", "/health" };

    public ApiResponseFormattingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        var isApiPath = path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
                        && !_excludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        if (!isApiPath)
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        string injectedJson = string.Empty;
        string bodyText = string.Empty;

        try
        {
            await _next(context);
        }
        finally
        {
            memStream.Seek(0, SeekOrigin.Begin);
            bodyText = await new StreamReader(memStream).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(bodyText))
            {
                injectedJson = context.Response.StatusCode switch
                {
                    401 => ApiResponse<object>.Unauthorized(
                        "Token tidak valid atau tidak ditemukan. Silakan login terlebih dahulu.").ToJson(),
                    403 => ApiResponse<object>.Forbidden(
                        "Anda tidak memiliki izin untuk mengakses resource ini.").ToJson(),
                    404 => ApiResponse<object>.NotFound("Resource").ToJson(),
                    405 => ApiResponse<object>.Error("Method Not Allowed",
                        $"Method {context.Request.Method} tidak diizinkan untuk endpoint ini.", 405).ToJson(),
                    _ => string.Empty
                };
            }

            context.Response.Body = originalBody;
        }

        if (!string.IsNullOrEmpty(injectedJson))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(injectedJson);
            return;
        }

        memStream.Seek(0, SeekOrigin.Begin);
        await memStream.CopyToAsync(originalBody);
    }
}