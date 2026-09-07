using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DynamicDbManager.Server.Data;
using DynamicDbManager.Server.Models;
using System.Security.Claims;

namespace DynamicDbManager.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttachmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AttachmentsController> _logger;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedContentTypes = {
        "image/jpeg", "image/png", "image/gif", "application/pdf",
        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain", "application/zip"
    };

    public AttachmentsController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AttachmentsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("upload/{rowId}")]
    public async Task<IActionResult> Upload(int rowId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не выбран");

        // Проверка размера
        if (file.Length > MaxFileSize)
            return BadRequest($"Размер файла превышает максимально допустимый ({MaxFileSize / 1024 / 1024} MB)");

        // Проверка типа содержимого (опционально)
        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest($"Тип файла '{file.ContentType}' не поддерживается. Разрешены: {string.Join(", ", AllowedContentTypes)}");

        var row = await _context.AdminTableRows
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == rowId);
        if (row == null)
            return NotFound("Запись не найдена");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var canEdit = await UserCanEditTable(userId, row.TableId);
        if (!canEdit)
            return Forbid("У вас нет прав на редактирование этой таблицы");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var attachment = new Attachment
        {
            RowId = rowId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FileData = memoryStream.ToArray(),
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = userId
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Файл {FileName} загружен для записи {RowId} пользователем {UserId}",
            file.FileName, rowId, userId);

        return Ok(new { id = attachment.Id, fileName = attachment.FileName });
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var attachment = await _context.Attachments
            .Include(a => a.Row)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (attachment == null)
            return NotFound("Вложение не найдено");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var canView = await UserCanViewTable(userId, attachment.Row.TableId);
        if (!canView)
            return Forbid("У вас нет прав на просмотр этой таблицы");

        _logger.LogInformation("Скачивание файла {FileName} (ID {Id}) пользователем {UserId}",
            attachment.FileName, id, userId);

        return File(attachment.FileData, attachment.ContentType, attachment.FileName);
    }

    [HttpGet("row/{rowId}")]
    public async Task<IActionResult> GetAttachments(int rowId)
    {
        var row = await _context.AdminTableRows.FindAsync(rowId);
        if (row == null)
            return NotFound("Запись не найдена");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var canView = await UserCanViewTable(userId, row.TableId);
        if (!canView)
            return Forbid("У вас нет прав на просмотр этой таблицы");

        var attachments = await _context.Attachments
            .Where(a => a.RowId == rowId)
            .Select(a => new { a.Id, a.FileName, a.ContentType, a.FileSize, a.UploadedAt })
            .ToListAsync();
        return Ok(attachments);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var attachment = await _context.Attachments
            .Include(a => a.Row)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (attachment == null)
            return NotFound("Вложение не найдено");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var canEdit = await UserCanEditTable(userId, attachment.Row.TableId);
        if (!canEdit)
            return Forbid("У вас нет прав на редактирование этой таблицы");

        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Файл {FileName} (ID {Id}) удалён пользователем {UserId}",
            attachment.FileName, id, userId);

        return NoContent();
    }

    private async Task<bool> UserCanEditTable(string userId, int tableId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (await _userManager.IsInRoleAsync(user, "Admin")) return true;
        return await _context.UserTablePermissions
            .AnyAsync(p => p.UserId == userId && p.TableId == tableId && p.CanEdit);
    }

    private async Task<bool> UserCanViewTable(string userId, int tableId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (await _userManager.IsInRoleAsync(user, "Admin")) return true;
        return await _context.UserTablePermissions
            .AnyAsync(p => p.UserId == userId && p.TableId == tableId && p.CanView);
    }
}