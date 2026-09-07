using System.ComponentModel.DataAnnotations;

namespace DynamicDbManager.Server.Models;

public class Attachment
{
    public int Id { get; set; }

    public int RowId { get; set; } // связь с AdminTableRow

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public byte[] FileData { get; set; } = Array.Empty<byte>(); // храним в БД

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public string? UploadedByUserId { get; set; } // кто загрузил

    // Навигационное свойство
    public AdminTableRow Row { get; set; } = null!;
}