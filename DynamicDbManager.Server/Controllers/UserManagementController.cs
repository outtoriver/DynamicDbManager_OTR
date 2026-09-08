using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DynamicDbManager.Server.Data;
using DynamicDbManager.Server.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DynamicDbManager.Server.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/users")]
public class UserManagementController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserManagementController> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .ToListAsync(cancellationToken);

        var result = new List<object>(users.Count);
        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new { user.Id, user.UserName, user.Email, Roles = roles });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var roles = NormalizeRoles(dto.Roles);
        if (roles.Count == 0) roles.Add("User");

        var invalidRole = await FindInvalidRoleAsync(roles);
        if (invalidRole != null)
            return BadRequest(new { message = $"Роль '{invalidRole}' не существует." });

        var user = new ApplicationUser
        {
            UserName = dto.UserName.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim()
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var roleResult = await _userManager.AddToRolesAsync(user, roles);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(roleResult.Errors);
        }

        await transaction.CommitAsync(cancellationToken);
        _logger.LogInformation("Создан пользователь {UserId} ({UserName}) с ролями {Roles}", user.Id, user.UserName, string.Join(", ", roles));

        return Ok(new { user.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentRoles = await _userManager.GetRolesAsync(user);
        var isTargetAdmin = currentRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

        List<string>? requestedRoles = null;
        if (dto.Roles != null)
        {
            requestedRoles = NormalizeRoles(dto.Roles);
            var invalidRole = await FindInvalidRoleAsync(requestedRoles);
            if (invalidRole != null)
                return BadRequest(new { message = $"Роль '{invalidRole}' не существует." });
        }

        if (isTargetAdmin && requestedRoles != null &&
            !requestedRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase) &&
            await IsLastAdminAsync(user.Id, cancellationToken))
        {
            return Conflict(new { message = "Нельзя снять роль Admin с последнего администратора." });
        }

        if (string.Equals(currentUserId, id, StringComparison.OrdinalIgnoreCase) &&
            requestedRoles != null &&
            !requestedRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Нельзя снять роль Admin с собственной учётной записи." });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        user.Email = string.IsNullOrWhiteSpace(dto.Email) ? user.Email : dto.Email.Trim();
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (requestedRoles != null)
        {
            var rolesToRemove = currentRoles.Except(requestedRoles, StringComparer.OrdinalIgnoreCase).ToArray();
            var rolesToAdd = requestedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);
            }

            if (rolesToAdd.Length > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded) return BadRequest(addResult.Errors);
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!resetResult.Succeeded) return BadRequest(resetResult.Errors);
        }

        await transaction.CommitAsync(cancellationToken);
        _logger.LogInformation("Пользователь {UserId} обновлён администратором {AdminUserId}", id, currentUserId);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(currentUserId, id, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Нельзя удалить собственную учётную запись." });

        if (await _userManager.IsInRoleAsync(user, "Admin") && await IsLastAdminAsync(user.Id, cancellationToken))
            return Conflict(new { message = "Нельзя удалить последнего администратора." });

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        _logger.LogInformation("Пользователь {UserId} удалён администратором {AdminUserId}", id, currentUserId);
        return NoContent();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        _logger.LogInformation("Пароль пользователя {UserId} сброшен администратором {AdminUserId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
        return Ok();
    }

    private async Task<string?> FindInvalidRoleAsync(IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                return role;
        }

        return null;
    }

    private async Task<bool> IsLastAdminAsync(string userId, CancellationToken cancellationToken)
    {
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole == null) return false;

        var adminCount = await _context.UserRoles
            .AsNoTracking()
            .CountAsync(ur => ur.RoleId == adminRole.Id, cancellationToken);

        return adminCount <= 1;
    }

    private static List<string> NormalizeRoles(IEnumerable<string>? roles)
        => (roles ?? Enumerable.Empty<string>())
            .Select(role => role?.Trim() ?? string.Empty)
            .Where(role => role.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}

public class CreateUserDto
{
    [Required, MinLength(2), MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    [Required, MinLength(10)]
    public string Password { get; set; } = string.Empty;

    public List<string>? Roles { get; set; }
}

public class UpdateUserDto
{
    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    public List<string>? Roles { get; set; }

    [MinLength(10)]
    public string? NewPassword { get; set; }
}

public class ResetPasswordDto
{
    [Required, MinLength(10)]
    public string NewPassword { get; set; } = string.Empty;
}
