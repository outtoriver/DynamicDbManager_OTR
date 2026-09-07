using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DynamicDbManager.Server.Data;
using DynamicDbManager.Server.Models;
using System.Security.Claims;
using System.Text.Json;

namespace DynamicDbManager.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class AdminTablesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminTablesController> _logger;

    public AdminTablesController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminTablesController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<List<AdminTable>> GetAccessibleTables(string userId, bool forEdit = false)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<AdminTable>();

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        IQueryable<AdminTable> query = _context.AdminTables;

        if (!isAdmin)
        {
            query = from t in query
                    join p in _context.UserTablePermissions on t.Id equals p.TableId
                    where p.UserId == userId && (forEdit ? p.CanEdit : p.CanView)
                    select t;
        }

        return await query.ToListAsync();
    }

    private async Task<bool> UserCanEditTable(string userId, int tableId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (await _userManager.IsInRoleAsync(user, "Admin")) return true;
        return await _context.UserTablePermissions
            .AnyAsync(p => p.UserId == userId && p.TableId == tableId && p.CanEdit);
    }

    // ========== Таблицы ==========
    [HttpGet]
    public async Task<IActionResult> GetTables()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var tables = await GetAccessibleTables(userId);

        var result = tables.Select(t => new
        {
            t.Id,
            t.Name,
            TableColumns = string.IsNullOrEmpty(t.TableColumnsJson)
                ? new List<TableColumn>()
                : JsonSerializer.Deserialize<List<TableColumn>>(t.TableColumnsJson),
            ModalColumns = string.IsNullOrEmpty(t.ModalColumnsJson)
                ? new List<ModalColumn>()
                : JsonSerializer.Deserialize<List<ModalColumn>>(t.ModalColumnsJson),
            t.CreatedAt
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTable([FromBody] AdminTableDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
            return Forbid("Только администратор может создавать таблицы");

        var table = new AdminTable
        {
            Name = dto.Name,
            TableColumnsJson = JsonSerializer.Serialize(dto.TableColumns ?? new List<TableColumn>()),
            ModalColumnsJson = JsonSerializer.Serialize(dto.ModalColumns ?? new List<ModalColumn>())
        };
        _context.AdminTables.Add(table);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Таблица {Name} создана пользователем {UserId}", dto.Name, userId);

        return Ok(new { table.Id, table.Name });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTable(int id, [FromBody] AdminTableDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
            return Forbid("Только администратор может изменять структуру таблицы");

        var table = await _context.AdminTables.FindAsync(id);
        if (table == null) return NotFound();

        table.Name = dto.Name;
        table.TableColumnsJson = JsonSerializer.Serialize(dto.TableColumns ?? new List<TableColumn>());
        table.ModalColumnsJson = JsonSerializer.Serialize(dto.ModalColumns ?? new List<ModalColumn>());
        await _context.SaveChangesAsync();

        _logger.LogInformation("Таблица {Name} (ID {Id}) обновлена пользователем {UserId}", dto.Name, id, userId);

        return Ok(new { table.Id, table.Name });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTable(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
            return Forbid("Только администратор может удалять таблицы");

        var table = await _context.AdminTables.Include(t => t.Rows).FirstOrDefaultAsync(t => t.Id == id);
        if (table == null) return NotFound();

        _context.AdminTables.Remove(table);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Таблица {Name} (ID {Id}) удалена пользователем {UserId}", table.Name, id, userId);

        return NoContent();
    }

    // ========== Строки с серверной пагинацией и поиском ==========
    [HttpGet("{tableId}/rows")]
    public async Task<IActionResult> GetRows(
        int tableId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        [FromQuery] string? search = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var canView = await _context.UserTablePermissions
                .AsNoTracking()
                .AnyAsync(p => p.UserId == userId && p.TableId == tableId && p.CanView);

            if (!canView)
                return Forbid("У вас нет прав на просмотр этой таблицы");
        }

        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 1000);
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        IQueryable<AdminTableRow> query = _context.AdminTableRows
            .AsNoTracking()
            .Where(r => r.TableId == tableId);

        // DataJson хранится целиком в одной колонке.
        // Contains переводится SQL Server в серверную операцию поиска,
        // поэтому больше не загружаем всю таблицу в память.
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r => r.DataJson.Contains(search));
        }

        var total = await query.CountAsync();

        var rows = await query
            .OrderBy(r => r.Id)
            .Skip(skip)
            .Take(take)
            .Select(r => new
            {
                r.Id,
                r.TableId,
                r.DataJson
            })
            .ToListAsync();

        return Ok(new { total, rows });
    }

    // ========== Глобальный поиск ==========
    [HttpGet("search")]
    public async Task<IActionResult> GlobalSearch([FromQuery] string q)
    {
        q = q?.Trim() ?? string.Empty;
        if (q.Length < 2)
            return Ok(Array.Empty<object>());

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        IQueryable<AdminTable> accessibleTables = _context.AdminTables.AsNoTracking();

        if (!isAdmin)
        {
            accessibleTables =
                from t in _context.AdminTables.AsNoTracking()
                join p in _context.UserTablePermissions.AsNoTracking()
                    on t.Id equals p.TableId
                where p.UserId == userId && p.CanView
                select t;
        }

        var results = await (
            from r in _context.AdminTableRows.AsNoTracking()
            join t in accessibleTables on r.TableId equals t.Id
            where r.DataJson.Contains(q)
            orderby r.Id
            select new
            {
                r.Id,
                r.TableId,
                r.DataJson,
                TableName = t.Name
            })
            .Take(100)
            .ToListAsync();

        return Ok(results);
    }

    // ========== Создание/обновление/удаление строк ==========
    [HttpPost("{tableId}/rows")]
    public async Task<IActionResult> CreateRow(int tableId, [FromBody] Dictionary<string, object?> data)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!await UserCanEditTable(userId, tableId))
            return Forbid("У вас нет прав на редактирование этой таблицы");

        var table = await _context.AdminTables.FindAsync(tableId);
        if (table == null) return NotFound();

        var row = new AdminTableRow
        {
            TableId = tableId,
            DataJson = JsonSerializer.Serialize(data)
        };
        _context.AdminTableRows.Add(row);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Строка создана в таблице {TableId} пользователем {UserId}", tableId, userId);

        return Ok(row);
    }

    [HttpPut("rows/{rowId}")]
    public async Task<IActionResult> UpdateRow(int rowId, [FromBody] Dictionary<string, object?> data)
    {
        var row = await _context.AdminTableRows.FindAsync(rowId);
        if (row == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!await UserCanEditTable(userId, row.TableId))
            return Forbid("У вас нет прав на редактирование этой таблицы");

        row.DataJson = JsonSerializer.Serialize(data);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Строка {RowId} обновлена пользователем {UserId}", rowId, userId);

        return Ok(row);
    }

    [HttpDelete("rows/{rowId}")]
    public async Task<IActionResult> DeleteRow(int rowId)
    {
        var row = await _context.AdminTableRows.FindAsync(rowId);
        if (row == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        if (!await UserCanEditTable(userId, row.TableId))
            return Forbid("У вас нет прав на удаление записей в этой таблице");

        _context.AdminTableRows.Remove(row);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Строка {RowId} удалена пользователем {UserId}", rowId, userId);

        return NoContent();
    }
}

public class AdminTableDto
{
    public string Name { get; set; } = string.Empty;
    public List<TableColumn>? TableColumns { get; set; }
    public List<ModalColumn>? ModalColumns { get; set; }
}