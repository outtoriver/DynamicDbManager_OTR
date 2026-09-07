using DynamicDbManager.Server.Data;
using DynamicDbManager.Server.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;

using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// REQUEST SIZE
// ============================================================

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
});

// ============================================================
// IDENTITY
// ============================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ============================================================
// JWT
// ============================================================

var jwtSettings = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSettings["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key не задан в конфигурации."
    );
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(key),

            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ============================================================
// CONTROLLERS
// ============================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            JsonNamingPolicy.CamelCase;

        options.JsonSerializerOptions.PropertyNameCaseInsensitive =
            true;

        options.JsonSerializerOptions.DictionaryKeyPolicy =
            JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// APP
// ============================================================

var app = builder.Build();

// ============================================================
// MIDDLEWARE
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

// ============================================================
// STATIC FILES
// ============================================================

app.UseDefaultFiles();
app.UseStaticFiles();

// ============================================================
// AUTH
// ============================================================

app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// API
// ============================================================

app.MapControllers();

// ============================================================
// IMPORTANT:
// SPA FALLBACK
//
// Vue использует createWebHistory().
// Поэтому прямой запрос:
//     /table/1
//
// должен возвращать index.html,
// после чего Vue Router обработает /table/1.
// ============================================================

app.MapFallbackToFile("index.html");

// ============================================================
// DATABASE INITIALIZATION
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    var logger =
        scope.ServiceProvider
            .GetRequiredService<ILogger<Program>>();

    try
    {
        dbContext.Database.Migrate();

        logger.LogInformation(
            "Миграции успешно применены."
        );
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "Ошибка при применении миграций."
        );
    }

    // ========================================================
    // ROLES
    // ========================================================

    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles =
    {
        "Admin",
        "User"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(
                new IdentityRole(role)
            );
        }
    }

    // ========================================================
    // ADMIN
    // ========================================================

    var adminConfig =
        builder.Configuration.GetSection("AdminUser");

    var adminUserName =
        adminConfig["UserName"] ?? "admin";

    var adminEmail =
        adminConfig["Email"] ??
        "admin@example.com";

    var adminPassword =
        adminConfig["Password"] ??
        "Admin123!";

    var adminUser =
        await userManager.FindByNameAsync(
            adminUserName
        );

    if (adminUser == null)
    {
        adminUser =
            new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail
            };

        var createResult =
            await userManager.CreateAsync(
                adminUser,
                adminPassword
            );

        if (!createResult.Succeeded)
        {
            logger.LogError(
                "Не удалось создать администратора: {Errors}",
                string.Join(
                    ", ",
                    createResult.Errors.Select(
                        e => e.Description
                    )
                )
            );
        }
        else
        {
            await userManager.AddToRoleAsync(
                adminUser,
                "Admin"
            );

            logger.LogInformation(
                "Администратор {AdminUserName} создан.",
                adminUserName
            );
        }
    }
}

// ============================================================

app.Run();