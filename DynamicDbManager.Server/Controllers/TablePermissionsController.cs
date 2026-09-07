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
    private readonly ILogger<TablePermissionsController> _logger;

    public TablePermissionsController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<TablePermissionsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var permissions = await _context.UserTablePermissions
                .AsNoTracking()
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
                .OrderBy(p => p.UserName)
                .ThenBy(p => p.TableName)
                .ToListAsync();

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка загрузки прав доступа к таблицам");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Не удалось загрузить права доступа. Проверьте структуру базы данных."
                });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPermissions(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("Не указан идентификатор пользователя");

        try
        {
            var userExists = await _userManager.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == userId);

            if (!userExists)
                return NotFound("Пользователь не найден");

            var permissions = await _context.UserTablePermissions
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.TableId,
                    p.CanView,
                    p.CanEdit,
                    p.CanDelete
                })
                .OrderBy(p => p.TableId)
                .ToListAsync();

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка загрузки прав пользователя {UserId}",
                userId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Не удалось загрузить права пользователя."
                });
        }
    }

    [HttpPost("batch")]
    public async Task<IActionResult> SetBatchPermissions(
        [FromBody] BatchSetPermissionsDto? dto)
    {
        if (dto == null)
            return BadRequest("Тело запроса не заполнено");

        if (string.IsNullOrWhiteSpace(dto.UserId))
            return BadRequest("Не указан пользователь");

        if (dto.Permissions == null || dto.Permissions.Count == 0)
            return BadRequest("Не выбрана ни одна таблица");

        var user = await _userManager.FindByIdAsync(dto.UserId);

        if (user == null)
            return NotFound("Пользователь не найден");

        // Убираем дубликаты таблиц.
        var permissions = dto.Permissions
            .GroupBy(p => p.TableId)
            .Select(g => g.First())
            .ToList();

        if (permissions.Any(p => p.TableId <= 0))
        {
            return BadRequest(
                "Передан некорректный идентификатор таблицы");
        }

        var tableIds = permissions
            .Select(p => p.TableId)
            .Distinct()
            .ToList();

        try
        {
            // Проверяем, что все таблицы реально существуют.
            var tables = await _context.AdminTables
                .AsNoTracking()
                .Where(t => tableIds.Contains(t.Id))
                .Select(t => new { t.Id })
                .ToListAsync();

            var existingTableIds = tables
                .Select(t => t.Id)
                .ToHashSet();

            var missingTableIds = tableIds
                .Where(id => !existingTableIds.Contains(id))
                .ToList();

            if (missingTableIds.Count > 0)
            {
                return NotFound(new
                {
                    message = "Одна или несколько таблиц не найдены",
                    tableIds = missingTableIds
                });
            }

            /*
             * ВАЖНО:
             *
             * Старый код делал:
             *
             *   DELETE существующих записей
             *   INSERT тех же записей
             *
             * При UNIQUE INDEX:
             *
             *   (UserId, TableId)
             *
             * EF Core мог сформировать SQL-команды так,
             * что INSERT выполнялся до DELETE, что давало:
             *
             *   Cannot insert duplicate key...
             *
             * Теперь существующие записи просто обновляются,
             * а отсутствующие создаются.
             */

            var existingPermissions =
                await _context.UserTablePermissions
                    .Where(p =>
                        p.UserId == dto.UserId &&
                        tableIds.Contains(p.TableId))
                    .ToListAsync();

            var existingByTableId = existingPermissions
                .ToDictionary(p => p.TableId);

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            foreach (var item in permissions)
            {
                if (existingByTableId.TryGetValue(
                        item.TableId,
                        out var existing))
                {
                    existing.CanView = item.CanView;
                    existing.CanEdit = item.CanEdit;
                    existing.CanDelete = item.CanDelete;
                }
                else
                {
                    _context.UserTablePermissions.Add(
                        new UserTablePermission
                        {
                            UserId = dto.UserId,
                            TableId = item.TableId,
                            CanView = item.CanView,
                            CanEdit = item.CanEdit,
                            CanDelete = item.CanDelete
                        });
                }
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation(
                "Права назначены пользователю {UserId} для {Count} таблиц",
                dto.UserId,
                permissions.Count);

            return Ok(new
            {
                message = "Права успешно назначены",
                userId = dto.UserId,
                updatedTables = permissions.Count
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Ошибка БД при массовом назначении прав пользователю {UserId}. Таблицы: {TableIds}",
                dto.UserId,
                string.Join(", ", tableIds));

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Не удалось сохранить права. Проверьте, что структура базы данных соответствует текущей модели."
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка массового назначения прав пользователю {UserId}",
                dto.UserId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Произошла ошибка при назначении прав."
                });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SetPermission(
        [FromBody] SetPermissionDto? dto)
    {
        if (dto == null)
            return BadRequest("Тело запроса не заполнено");

        if (string.IsNullOrWhiteSpace(dto.UserId))
            return BadRequest("Не указан пользователь");

        if (dto.TableId <= 0)
            return BadRequest(
                "Некорректный идентификатор таблицы");

        var user = await _userManager.FindByIdAsync(dto.UserId);

        if (user == null)
            return NotFound("Пользователь не найден");

        var tableExists = await _context.AdminTables
            .AsNoTracking()
            .AnyAsync(t => t.Id == dto.TableId);

        if (!tableExists)
            return NotFound("Таблица не найдена");

        try
        {
            var existing =
                await _context.UserTablePermissions
                    .FirstOrDefaultAsync(
                        p =>
                            p.UserId == dto.UserId &&
                            p.TableId == dto.TableId);

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

            _logger.LogInformation(
                "Право пользователя {UserId} на таблицу {TableId} сохранено",
                dto.UserId,
                dto.TableId);

            return Ok(new
            {
                id = existing.Id,
                userId = existing.UserId,
                tableId = existing.TableId,
                canView = existing.CanView,
                canEdit = existing.CanEdit,
                canDelete = existing.CanDelete
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Ошибка БД при сохранении права пользователя {UserId} на таблицу {TableId}",
                dto.UserId,
                dto.TableId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Не удалось сохранить право. Проверьте структуру базы данных."
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка сохранения права пользователя {UserId} на таблицу {TableId}",
                dto.UserId,
                dto.TableId);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Произошла ошибка при сохранении права."
                });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        if (id <= 0)
            return BadRequest(
                "Некорректный идентификатор права");

        try
        {
            var permission =
                await _context.UserTablePermissions.FindAsync(id);

            if (permission == null)
                return NotFound();

            _context.UserTablePermissions.Remove(permission);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Право {PermissionId} удалено",
                id);

            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Ошибка БД при удалении права {PermissionId}",
                id);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Не удалось удалить право."
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Ошибка удаления права {PermissionId}",
                id);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Произошла ошибка при удалении права."
                });
        }
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