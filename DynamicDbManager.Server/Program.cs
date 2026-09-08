using System.Text;
using System.Text.Json;
using DynamicDbManager.Server.Data;
using DynamicDbManager.Server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- Request limits ----------
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
});

// ---------- Identity ----------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 10;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ---------- JWT Authentication ----------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key должен быть задан и содержать не менее 32 байт.");

if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Jwt:Issuer и Jwt:Audience должны быть заданы.");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection не задан.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

// ---------- DB Context ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---------- Middleware ----------
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ---------- Database initialization ----------
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Миграции успешно применены.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Не удалось применить миграции. Приложение остановлено, чтобы не работать с несовместимой схемой БД.");
        throw;
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (await roleManager.RoleExistsAsync(role))
            continue;

        var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Не удалось создать роль {role}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }
    }

    var adminConfig = builder.Configuration.GetSection("AdminUser");
    var adminUserName = adminConfig["UserName"]?.Trim();
    var adminEmail = adminConfig["Email"]?.Trim();
    var adminPassword = adminConfig["Password"];

    if (string.IsNullOrWhiteSpace(adminUserName))
        throw new InvalidOperationException("AdminUser:UserName не задан.");

    var adminUser = await userManager.FindByNameAsync(adminUserName);
    if (adminUser == null)
    {
        if (string.IsNullOrWhiteSpace(adminEmail))
            throw new InvalidOperationException("AdminUser:Email обязателен при первоначальном создании администратора.");

        if (string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException("AdminUser:Password обязателен при первоначальном создании администратора.");

        adminUser = new ApplicationUser
        {
            UserName = adminUserName,
            Email = adminEmail
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Не удалось создать администратора: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!addRoleResult.Succeeded)
        {
            await userManager.DeleteAsync(adminUser);
            throw new InvalidOperationException(
                $"Не удалось назначить роль Admin новому администратору: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
        }

        logger.LogInformation("Администратор {AdminUserName} создан.", adminUserName);
    }
    else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Пользователь {adminUserName} существует, но ему не удалось назначить роль Admin: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
        }

        logger.LogWarning("Пользователю {AdminUserName} повторно назначена роль Admin.", adminUserName);
    }
}

app.Run();
