using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DynamicDbManager.Server.Data;
using DynamicDbManager.Server.Models;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace DynamicDbManager.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/excel")]
[RequestSizeLimit(52_428_800)] // 50 MiB
public class ExcelController : ControllerBase
{
    private const int PreviewRows = 18;
    private const int MaxImportRows = 250_000;

    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ExcelController> _logger;

    public ExcelController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ExcelController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview(
        IFormFile file,
        [FromForm] string? sheetName = null,
        [FromForm] string? range = null,
        CancellationToken cancellationToken = default)
    {
        var fileError = ValidateFile(file);
        if (fileError != null) return BadRequest(fileError);

        await using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);

        var worksheet = GetWorksheet(workbook, sheetName);
        var used = worksheet.RangeUsed();
        if (used == null)
            return BadRequest("Excel-лист пуст.");

        var selected = ResolveRange(worksheet, range, used);
        var firstColumn = selected.RangeAddress.FirstAddress.ColumnNumber;
        var lastColumn = selected.RangeAddress.LastAddress.ColumnNumber;
        var firstRow = selected.RangeAddress.FirstAddress.RowNumber;
        var lastRow = selected.RangeAddress.LastAddress.RowNumber;

        var headers = new List<string>();
        for (var column = firstColumn; column <= lastColumn; column++)
        {
            headers.Add(GetCellText(worksheet.Cell(firstRow, column)));
        }

        var preview = new List<List<string>>();
        for (var row = firstRow; row <= Math.Min(lastRow, firstRow + PreviewRows - 1); row++)
        {
            var values = new List<string>();
            for (var column = firstColumn; column <= lastColumn; column++)
                values.Add(GetCellText(worksheet.Cell(row, column)));
            preview.Add(values);
        }

        var sheets = workbook.Worksheets
            .Select(ws => new
            {
                name = ws.Name,
                usedRange = ws.RangeUsed()?.RangeAddress.ToString() ?? string.Empty
            })
            .ToList();

        return Ok(new
        {
            sheetName = worksheet.Name,
            usedRange = used.RangeAddress.ToString(),
            range = selected.RangeAddress.ToString(),
            rowCount = lastRow - firstRow + 1,
            columnCount = lastColumn - firstColumn + 1,
            headers,
            rows = preview,
            sheets
        });
    }

    [HttpPost("import/new-table")]
    public async Task<IActionResult> ImportNewTable(
        IFormFile file,
        [FromForm] string? name = null,
        [FromForm] string? sheetName = null,
        [FromForm] string? range = null,
        [FromForm] bool hasHeaders = true,
        CancellationToken cancellationToken = default)
    {
        var fileError = ValidateFile(file);
        if (fileError != null) return BadRequest(fileError);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();
        if (!await _userManager.IsInRoleAsync(user, "Admin"))
            return Forbid("Только администратор может создавать таблицы из Excel");

        await using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = GetWorksheet(workbook, sheetName);
        var used = worksheet.RangeUsed();
        if (used == null) return BadRequest("Excel-лист пуст.");

        var selected = ResolveRange(worksheet, range, used);
        var firstRow = selected.RangeAddress.FirstAddress.RowNumber;
        var lastRow = selected.RangeAddress.LastAddress.RowNumber;
        var firstColumn = selected.RangeAddress.FirstAddress.ColumnNumber;
        var lastColumn = selected.RangeAddress.LastAddress.ColumnNumber;

        if (!hasHeaders)
            return BadRequest("Для создания новой таблицы первая строка диапазона должна содержать заголовки колонок.");

        var headerNames = BuildUniqueHeaders(worksheet, firstRow, firstColumn, lastColumn);
        var dataStartRow = firstRow + 1;
        var dataRowCount = Math.Max(0, lastRow - dataStartRow + 1);

        if (headerNames.Count == 0)
            return BadRequest("Не удалось определить заголовки колонок.");
        if (dataRowCount > MaxImportRows)
            return BadRequest($"Файл содержит слишком много строк. Максимум: {MaxImportRows:N0}.");

        var tableName = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(tableName))
            tableName = Path.GetFileNameWithoutExtension(file.FileName).Trim();
        if (string.IsNullOrWhiteSpace(tableName))
            tableName = "Импорт Excel";
        if (tableName.Length > 200)
            tableName = tableName[..200];

        var columns = new List<TableColumn>();
        for (var i = 0; i < headerNames.Count; i++)
        {
            var isBoolean = InferBooleanColumn(worksheet, dataStartRow, lastRow, firstColumn + i);
            columns.Add(new TableColumn
            {
                Name = headerNames[i],
                Type = isBoolean ? "boolean" : "text"
            });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var table = new AdminTable
        {
            Name = tableName,
            TableColumnsJson = JsonSerializer.Serialize(columns),
            ModalColumnsJson = JsonSerializer.Serialize(columns.Select(c => new ModalColumn
            {
                Name = c.Name,
                Type = c.Type
            }).ToList())
        };

        _context.AdminTables.Add(table);
        await _context.SaveChangesAsync(cancellationToken);

        var imported = await AddRowsAsync(
            table.Id,
            worksheet,
            dataStartRow,
            lastRow,
            firstColumn,
            headerNames,
            columns,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Excel импорт: создана таблица {TableId} {TableName}, строк {Rows}, пользователь {UserId}",
            table.Id,
            table.Name,
            imported,
            userId);

        return Ok(new
        {
            tableId = table.Id,
            tableName = table.Name,
            importedRows = imported,
            columns = columns.Count
        });
    }

    [HttpPost("import/existing/{tableId:int}")]
    public async Task<IActionResult> ImportExisting(
        int tableId,
        IFormFile file,
        [FromForm] string? sheetName = null,
        [FromForm] string? range = null,
        [FromForm] bool hasHeaders = true,
        [FromForm] bool mapByHeader = true,
        CancellationToken cancellationToken = default)
    {
        var fileError = ValidateFile(file);
        if (fileError != null) return BadRequest(fileError);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        if (!await UserCanEditTable(userId, tableId))
            return Forbid("У вас нет прав на импорт данных в эту таблицу");

        var table = await _context.AdminTables.FindAsync([tableId], cancellationToken);
        if (table == null) return NotFound("Таблица не найдена.");

        var columns = DeserializeTableColumns(table.TableColumnsJson);
        if (columns.Count == 0)
            return BadRequest("У целевой таблицы нет колонок.");

        await using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = GetWorksheet(workbook, sheetName);
        var used = worksheet.RangeUsed();
        if (used == null) return BadRequest("Excel-лист пуст.");

        var selected = ResolveRange(worksheet, range, used);
        var firstRow = selected.RangeAddress.FirstAddress.RowNumber;
        var lastRow = selected.RangeAddress.LastAddress.RowNumber;
        var firstColumn = selected.RangeAddress.FirstAddress.ColumnNumber;
        var lastColumn = selected.RangeAddress.LastAddress.ColumnNumber;

        var sourceHeaders = new List<string>();
        if (hasHeaders)
        {
            for (var column = firstColumn; column <= lastColumn; column++)
                sourceHeaders.Add(GetCellText(worksheet.Cell(firstRow, column)).Trim());
        }

        var mapping = BuildMapping(columns, sourceHeaders, firstColumn, lastColumn, hasHeaders, mapByHeader);
        if (mapping.Count == 0)
            return BadRequest("Не удалось сопоставить колонки Excel с колонками таблицы.");

        var dataStartRow = hasHeaders ? firstRow + 1 : firstRow;
        var dataRowCount = Math.Max(0, lastRow - dataStartRow + 1);
        if (dataRowCount > MaxImportRows)
            return BadRequest($"Файл содержит слишком много строк. Максимум: {MaxImportRows:N0}.");

        var imported = 0;
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        imported = await AddMappedRowsAsync(
            tableId,
            worksheet,
            dataStartRow,
            lastRow,
            mapping,
            columns,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Excel импорт: таблица {TableId}, добавлено строк {Rows}, пользователь {UserId}",
            tableId,
            imported,
            userId);

        return Ok(new
        {
            tableId,
            importedRows = imported,
            mappedColumns = mapping.Count,
            totalColumns = columns.Count
        });
    }

    [HttpGet("export/{tableId:int}")]
    public async Task<IActionResult> Export(
        int tableId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        if (!await UserCanViewTable(userId, tableId))
            return Forbid("У вас нет прав на экспорт этой таблицы");

        var table = await _context.AdminTables
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tableId, cancellationToken);

        if (table == null) return NotFound("Таблица не найдена.");

        var columns = DeserializeTableColumns(table.TableColumnsJson);
        var rows = await _context.AdminTableRows
            .AsNoTracking()
            .Where(r => r.TableId == tableId)
            .OrderBy(r => r.Id)
            .Select(r => new { r.DataJson })
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet(MakeWorksheetName(table.Name));

        var columnCount = columns.Count;
        if (columnCount == 0)
            return BadRequest("У таблицы нет колонок для экспорта.");

        for (var i = 0; i < columnCount; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = columns[i].Name;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#111827");
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        var rowIndex = 2;
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Dictionary<string, JsonElement> data;
            try
            {
                data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(row.DataJson)
                    ?? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                data = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }

            for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                var column = columns[columnIndex];
                if (!data.TryGetValue(column.Name, out var value) || value.ValueKind == JsonValueKind.Null)
                    continue;

                var cell = worksheet.Cell(rowIndex, columnIndex + 1);
                WriteJsonValue(cell, value);
            }

            rowIndex++;
        }

        var usedRange = worksheet.Range(1, 1, Math.Max(1, rowIndex - 1), columnCount);
        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        usedRange.SetAutoFilter();
        worksheet.SheetView.FreezeRows(1);
        worksheet.Row(1).Height = 24;
        worksheet.Columns(1, columnCount).AdjustToContents(1, Math.Min(rowIndex, 500));

        for (var i = 1; i <= columnCount; i++)
        {
            if (worksheet.Column(i).Width > 45)
                worksheet.Column(i).Width = 45;
            if (worksheet.Column(i).Width < 10)
                worksheet.Column(i).Width = 10;
        }

        await using var output = new MemoryStream();
        workbook.SaveAs(output);
        output.Position = 0;

        var fileName = $"{SanitizeFileName(table.Name)}.xlsx";
        return File(
            output.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private async Task<int> AddRowsAsync(
        int tableId,
        IXLWorksheet worksheet,
        int startRow,
        int lastRow,
        int firstColumn,
        IReadOnlyList<string> headers,
        IReadOnlyList<TableColumn> columns,
        CancellationToken cancellationToken)
    {
        var imported = 0;
        var batch = new List<AdminTableRow>(500);

        for (var row = startRow; row <= lastRow; row++)
        {
            var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            var hasValue = false;

            for (var index = 0; index < headers.Count; index++)
            {
                var cell = worksheet.Cell(row, firstColumn + index);
                var value = ConvertCellValue(cell, columns[index].Type);
                if (value is string text && !string.IsNullOrWhiteSpace(text)) hasValue = true;
                else if (value is not null) hasValue = true;
                data[headers[index]] = value;
            }

            if (!hasValue) continue;

            batch.Add(new AdminTableRow
            {
                TableId = tableId,
                DataJson = JsonSerializer.Serialize(data)
            });

            if (batch.Count >= 500)
            {
                _context.AdminTableRows.AddRange(batch);
                await _context.SaveChangesAsync(cancellationToken);
                imported += batch.Count;
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            _context.AdminTableRows.AddRange(batch);
            await _context.SaveChangesAsync(cancellationToken);
            imported += batch.Count;
        }

        return imported;
    }

    private async Task<int> AddMappedRowsAsync(
        int tableId,
        IXLWorksheet worksheet,
        int startRow,
        int lastRow,
        IReadOnlyList<ColumnMapping> mapping,
        IReadOnlyList<TableColumn> columns,
        CancellationToken cancellationToken)
    {
        var imported = 0;
        var batch = new List<AdminTableRow>(500);

        for (var row = startRow; row <= lastRow; row++)
        {
            var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            var hasValue = false;

            foreach (var item in mapping)
            {
                var target = columns[item.TargetColumnIndex];
                var value = ConvertCellValue(worksheet.Cell(row, item.SourceColumnNumber), target.Type);
                if (value is string text && !string.IsNullOrWhiteSpace(text)) hasValue = true;
                else if (value is not null) hasValue = true;
                data[target.Name] = value;
            }

            if (!hasValue) continue;

            batch.Add(new AdminTableRow
            {
                TableId = tableId,
                DataJson = JsonSerializer.Serialize(data)
            });

            if (batch.Count >= 500)
            {
                _context.AdminTableRows.AddRange(batch);
                await _context.SaveChangesAsync(cancellationToken);
                imported += batch.Count;
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            _context.AdminTableRows.AddRange(batch);
            await _context.SaveChangesAsync(cancellationToken);
            imported += batch.Count;
        }

        return imported;
    }

    private List<ColumnMapping> BuildMapping(
        IReadOnlyList<TableColumn> targetColumns,
        IReadOnlyList<string> sourceHeaders,
        int firstColumn,
        int lastColumn,
        bool hasHeaders,
        bool mapByHeader)
    {
        var mapping = new List<ColumnMapping>();

        if (hasHeaders && mapByHeader)
        {
            var targetLookup = targetColumns
                .Select((column, index) => new { column.Name, index })
                .GroupBy(x => NormalizeKey(x.Name))
                .ToDictionary(g => g.Key, g => g.First().index, StringComparer.OrdinalIgnoreCase);

            for (var offset = 0; offset < sourceHeaders.Count; offset++)
            {
                var key = NormalizeKey(sourceHeaders[offset]);
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (targetLookup.TryGetValue(key, out var targetIndex))
                {
                    mapping.Add(new ColumnMapping(firstColumn + offset, targetIndex));
                }
            }

            return mapping;
        }

        var count = Math.Min(lastColumn - firstColumn + 1, targetColumns.Count);
        for (var offset = 0; offset < count; offset++)
            mapping.Add(new ColumnMapping(firstColumn + offset, offset));

        return mapping;
    }

    private static List<TableColumn> DeserializeTableColumns(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<TableColumn>();
        try
        {
            return JsonSerializer.Deserialize<List<TableColumn>>(json) ?? new List<TableColumn>();
        }
        catch
        {
            return new List<TableColumn>();
        }
    }

    private static List<string> BuildUniqueHeaders(
        IXLWorksheet worksheet,
        int row,
        int firstColumn,
        int lastColumn)
    {
        var result = new List<string>();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var column = firstColumn; column <= lastColumn; column++)
        {
            var raw = GetCellText(worksheet.Cell(row, column)).Trim();
            var baseName = string.IsNullOrWhiteSpace(raw)
                ? $"Колонка {column - firstColumn + 1}"
                : raw;

            var name = baseName;
            var suffix = 2;
            while (!used.Add(name))
            {
                name = $"{baseName} {suffix++}";
            }

            result.Add(name);
        }

        return result;
    }

    private static bool InferBooleanColumn(
        IXLWorksheet worksheet,
        int startRow,
        int lastRow,
        int column)
    {
        var seen = false;
        for (var row = startRow; row <= lastRow && row < startRow + 200; row++)
        {
            var text = GetCellText(worksheet.Cell(row, column)).Trim();
            if (string.IsNullOrWhiteSpace(text)) continue;
            seen = true;
            if (!IsBooleanText(text)) return false;
        }
        return seen;
    }

    private static object? ConvertCellValue(IXLCell cell, string type)
    {
        if (cell.IsEmpty()) return null;

        if (string.Equals(type, "boolean", StringComparison.OrdinalIgnoreCase))
        {
            var text = GetCellText(cell).Trim();
            if (TryParseBoolean(text, out var boolValue)) return boolValue;
            return null;
        }

        if (cell.DataType == XLDataType.DateTime)
            return cell.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        return GetCellText(cell);
    }

    private static void WriteJsonValue(IXLCell cell, JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                cell.Value = value.GetString() ?? string.Empty;
                break;
            case JsonValueKind.Number:
                if (value.TryGetDecimal(out var decimalValue)) cell.Value = decimalValue;
                else if (value.TryGetDouble(out var doubleValue)) cell.Value = doubleValue;
                else cell.Value = value.ToString();
                break;
            case JsonValueKind.True:
                cell.Value = true;
                break;
            case JsonValueKind.False:
                cell.Value = false;
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static string GetCellText(IXLCell cell)
    {
        if (cell.IsEmpty()) return string.Empty;
        return cell.GetFormattedString() ?? string.Empty;
    }

    private static bool IsBooleanText(string value) => TryParseBoolean(value, out _);

    private static bool TryParseBoolean(string value, out bool result)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "true":
            case "1":
            case "да":
            case "yes":
            case "y":
            case "+":
                result = true;
                return true;
            case "false":
            case "0":
            case "нет":
            case "no":
            case "n":
            case "-":
                result = false;
                return true;
            default:
                result = false;
                return false;
        }
    }

    private static IXLWorksheet GetWorksheet(XLWorkbook workbook, string? sheetName)
    {
        if (!string.IsNullOrWhiteSpace(sheetName))
        {
            var exact = workbook.Worksheets.FirstOrDefault(x =>
                string.Equals(x.Name, sheetName, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;
            throw new InvalidOperationException($"Лист '{sheetName}' не найден.");
        }

        return workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("В книге нет листов.");
    }

    private static IXLRange ResolveRange(IXLWorksheet worksheet, string? range, IXLRange used)
    {
        if (string.IsNullOrWhiteSpace(range)) return used;

        try
        {
            return worksheet.Range(range.Trim());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Некорректный диапазон Excel: {range}", ex);
        }
    }

    private async Task<bool> UserCanViewTable(string userId, int tableId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (await _userManager.IsInRoleAsync(user, "Admin")) return true;
        return await _context.UserTablePermissions
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.TableId == tableId && p.CanView);
    }

    private async Task<bool> UserCanEditTable(string userId, int tableId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        if (await _userManager.IsInRoleAsync(user, "Admin")) return true;
        return await _context.UserTablePermissions
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.TableId == tableId && p.CanEdit);
    }

    private static string? ValidateFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return "Excel-файл не выбран.";

        if (file.Length > 52_428_800)
            return "Размер Excel-файла не должен превышать 50 МБ.";

        var ext = Path.GetExtension(file.FileName);
        if (!string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase))
            return "Поддерживается формат Excel .xlsx.";

        return null;
    }

    private static string NormalizeKey(string? value)
        => string.Join(" ", (value ?? string.Empty).Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();

    private static string MakeWorksheetName(string name)
    {
        var result = string.IsNullOrWhiteSpace(name) ? "Table" : name.Trim();
        foreach (var invalid in new[] { ':', '\\', '/', '?', '*', '[', ']' })
            result = result.Replace(invalid, '_');
        return result.Length <= 31 ? result : result[..31];
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var result = new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(result) ? "table" : result;
    }

    private readonly record struct ColumnMapping(int SourceColumnNumber, int TargetColumnIndex);
}
