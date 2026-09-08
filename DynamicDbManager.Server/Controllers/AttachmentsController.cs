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
    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly Dictionary<string, string[]> AllowedContentTypesByExtension =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = new[] { "image/jpeg" },
            [".jpeg"] = new[] { "image/jpeg" },
            [".png"] = new[] { "image/png" },
            [".gif"] = new[] { "image/gif" },
            [".pdf"] = new[] { "application/pdf" },
            [".doc"] = new[] { "application/msword" },
            [".docx"] = new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            [".xls"] = new[] { "application/vnd.ms-excel" },
            [".xlsx"] = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            [".txt"] = new[] { "text/plain" },
            [".zip"] = new[] { "application/zip" }
        };

    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AttachmentsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    private static IActionResult Forbidden(string message)
        => new ObjectResult(new { message }) { StatusCode = StatusCodes.Status403Forbidden };

    [HttpPost("upload/{rowId:int}")]
    public async Task<IActionResult> Upload(int rowId, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не выбран");

        if (file.Length > MaxFileSize)
            return BadRequest($"Размер файла превышает максимально допустимый ({MaxFileSize / 1024 / 1024} MB)");

        var validationError = await ValidateFileAsync(file, cancellationToken);
        if (validationError != null)
            return BadRequest(validationError);

        var row = await _context.AdminTableRows
            .AsNoTracking()
            .Select(r => new { r.Id, r.TableId })
            .FirstOrDefaultAsync(r => r.Id == rowId, cancellationToken);
        if (row == null)
            return NotFound("Запись не найдена");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!await UserCanEditTable(userId, row.TableId, cancellationToken))
            return Forbidden("У вас нет прав на редактирование этой таблицы");

        await using var memoryStream = new MemoryStream(capacity: checked((int)Math.Min(file.Length, int.MaxValue)));
        await file.CopyToAsync(memoryStream, cancellationToken);

        var safeFileName = SanitizeFileName(file.FileName);
        var attachment = new Attachment
        {
            RowId = rowId,
            FileName = safeFileName,
            ContentType = file.ContentType.Trim(),
            FileSize = file.Length,
            FileData = memoryStream.ToArray(),
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = userId
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Файл {FileName} загружен для записи {RowId} пользователем {UserId}",
            safeFileName, rowId, userId);

        return Ok(new { id = attachment.Id, fileName = attachment.FileName });
    }

    [HttpGet("download/{id:int}")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var attachment = await _context.Attachments
            .AsNoTracking()
            .Include(a => a.Row)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (attachment == null)
            return NotFound("Вложение не найдено");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!await UserCanViewTable(userId, attachment.Row.TableId, cancellationToken))
            return Forbidden("У вас нет прав на просмотр этой таблицы");

        _logger.LogInformation(
            "Скачивание файла {FileName} (ID {Id}) пользователем {UserId}",
            attachment.FileName, id, userId);

        return File(attachment.FileData, attachment.ContentType, attachment.FileName, enableRangeProcessing: true);
    }

    [HttpGet("row/{rowId:int}")]
    public async Task<IActionResult> GetAttachments(int rowId, CancellationToken cancellationToken)
    {
        var row = await _context.AdminTableRows
            .AsNoTracking()
            .Select(r => new { r.Id, r.TableId })
            .FirstOrDefaultAsync(r => r.Id == rowId, cancellationToken);
        if (row == null)
            return NotFound("Запись не найдена");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!await UserCanViewTable(userId, row.TableId, cancellationToken))
            return Forbidden("У вас нет прав на просмотр этой таблицы");

        var attachments = await _context.Attachments
            .AsNoTracking()
            .Where(a => a.RowId == rowId)
            .OrderBy(a => a.Id)
            .Select(a => new { a.Id, a.FileName, a.ContentType, a.FileSize, a.UploadedAt })
            .ToListAsync(cancellationToken);

        return Ok(attachments);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var attachment = await _context.Attachments
            .Include(a => a.Row)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (attachment == null)
            return NotFound("Вложение не найдено");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!await UserCanDeleteRows(userId, attachment.Row.TableId, cancellationToken))
            return Forbidden("У вас нет прав на удаление вложений в этой таблице");

        var fileName = attachment.FileName;
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Файл {FileName} (ID {Id}) удалён пользователем {UserId}",
            fileName, id, userId);

        return NoContent();
    }

    private async Task<bool> UserCanEditTable(string userId, int tableId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (await _userManager.IsInRoleAsync(user, "Admin")) return true;

        return await _context.UserTablePermissions
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.TableId == tableId && p.CanEdit, cancellationToken);
    }

    private async Task<bool> UserCanDeleteRows(string userId, int tableId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (await _userManager.IsInRoleAsync(user, "Admin")) return true;

        return await _context.UserTablePermissions
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.TableId == tableId && p.CanDelete, cancellationToken);
    }

    private async Task<bool> UserCanViewTable(string userId, int tableId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (await _userManager.IsInRoleAsync(user, "Admin")) return true;

        return await _context.UserTablePermissions
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.TableId == tableId && p.CanView, cancellationToken);
    }

    private static async Task<string?> ValidateFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedContentTypesByExtension.TryGetValue(extension, out var allowedTypes))
            return "Тип или расширение файла не поддерживается.";

        var contentType = (file.ContentType ?? string.Empty).Trim();
        if (!allowedTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return $"Тип файла '{contentType}' не соответствует расширению '{extension}'.";

        if (string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase))
            return null;

        await using var stream = file.OpenReadStream();
        var header = new byte[16];
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        if (!HasValidSignature(extension, header, read))
            return "Содержимое файла не соответствует заявленному формату.";

        return null;
    }

    private static bool HasValidSignature(string extension, byte[] header, int length)
    {
        static bool StartsWith(byte[] data, int count, params byte[] signature)
            => count >= signature.Length && data.AsSpan(0, signature.Length).SequenceEqual(signature);

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => StartsWith(header, length, 0xFF, 0xD8, 0xFF),
            ".png" => StartsWith(header, length, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A),
            ".gif" => StartsWith(header, length, (byte)'G', (byte)'I', (byte)'F') &&
                      (length >= 6 && (header[3] == (byte)'8' && (header[4] == (byte)'7' || header[4] == (byte)'9') && header[5] == (byte)'a')),
            ".pdf" => StartsWith(header, length, (byte)'%', (byte)'P', (byte)'D', (byte)'F'),
            ".doc" or ".xls" => StartsWith(header, length, 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1),
            ".docx" or ".xlsx" or ".zip" => StartsWith(header, length, 0x50, 0x4B, 0x03, 0x04) ||
                                                  StartsWith(header, length, 0x50, 0x4B, 0x05, 0x06) ||
                                                  StartsWith(header, length, 0x50, 0x4B, 0x07, 0x08),
            _ => false
        };
    }

    private static string SanitizeFileName(string value)
    {
        var name = Path.GetFileName(value ?? string.Empty);
        name = new string(name.Where(ch => !char.IsControl(ch)).ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "file";
        return name.Length <= 255 ? name : name[..255];
    }
}
