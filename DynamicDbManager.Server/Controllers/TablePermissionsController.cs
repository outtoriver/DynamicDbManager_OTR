using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DynamicDbManager.Server.Data;
using DynamicDbManager.Server.Models;

namespace DynamicDbManager.Server.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/[controller]")]
public class TablePermissionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TablePermissionsController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _context.UserTablePermissions
            .Select(p => new
            {
                p.Id,
                UserId = p.UserId,
                UserName = p.User.UserName,
                TableId = p.TableId,
                TableName = p.Table.Name,
                p.CanView,
                p.CanEdit,
                p.CanDelete
            })
            .ToListAsync();
        return Ok(permissions);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPermissions(string userId)
    {
        var permissions = await _context.UserTablePermissions
            .Where(p => p.UserId == userId)
            .Select(p => new
            {
                p.TableId,
                p.CanView,
                p.CanEdit,
                p.CanDelete
            })
            .ToListAsync();
        return Ok(permissions);
    }

    [HttpPost("batch")]
    public async Task<IActionResult> SetBatchPermissions([FromBody] BatchSetPermissionsDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return NotFound("Пользователь не найден");

        // Убираем дубликаты по TableId
        var permissions = (dto.Permissions ?? new List<PermissionItem>())
            .GroupBy(p => p.TableId)
            .Select(g => g.First())
            .ToList();

        var tableIds = permissions.Select(p => p.TableId).Distinct().ToList();
        if (tableIds.Count > 0)
        {
            var existingTableCount = await _context.AdminTables
                .CountAsync(t => tableIds.Contains(t.Id));
            if (existingTableCount != tableIds.Count)
                return NotFound("Одна или несколько таблиц не найдены");
        }

        // Batch is a full replacement for one user's permissions. Empty input
        // intentionally clears all permissions for that user.
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var existing = await _context.UserTablePermissions
            .Where(p => p.UserId == dto.UserId)
            .ToListAsync();
        _context.UserTablePermissions.RemoveRange(existing);

        foreach (var perm in permissions)
        {
            _context.UserTablePermissions.Add(new UserTablePermission
            {
                UserId = dto.UserId,
                TableId = perm.TableId,
                CanView = perm.CanView,
                CanEdit = perm.CanEdit,
                CanDelete = perm.CanDelete
            });
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SetPermission([FromBody] SetPermissionDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null) return NotFound("Пользователь не найден");

        var table = await _context.AdminTables.FindAsync(dto.TableId);
        if (table == null) return NotFound("Таблица не найдена");

        var existing = await _context.UserTablePermissions
            .FirstOrDefaultAsync(p => p.UserId == dto.UserId && p.TableId == dto.TableId);

        if (existing == null)
        {
            existing = new UserTablePermission
            {
                UserId = dto.UserId,
                TableId = dto.TableId,
                CanView = dto.CanView,
                CanEdit = dto.CanEdit,
                CanDelete = dto.CanDelete
            };
            _context.UserTablePermissions.Add(existing);
        }
        else
        {
            existing.CanView = dto.CanView;
            existing.CanEdit = dto.CanEdit;
            existing.CanDelete = dto.CanDelete;
        }

        await _context.SaveChangesAsync();
        return Ok(new { id = existing.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        var permission = await _context.UserTablePermissions.FindAsync(id);
        if (permission == null)
            return NotFound();
        _context.UserTablePermissions.Remove(permission);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    public class SetPermissionDto
    {
        public string UserId { get; set; } = string.Empty;
        public int TableId { get; set; }
        public bool CanView { get; set; } = true;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
    }

    public class BatchSetPermissionsDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<PermissionItem> Permissions { get; set; } = new();
    }

    public class PermissionItem
    {
        public int TableId { get; set; }
        public bool CanView { get; set; } = true;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
    }
}