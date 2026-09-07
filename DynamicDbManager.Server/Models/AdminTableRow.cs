using System.ComponentModel.DataAnnotations;

namespace DynamicDbManager.Server.Models;

public class AdminTableRow
{
    public int Id { get; set; }

    public int TableId { get; set; }

    public string DataJson { get; set; } = "{}";

    // Новое: коллекция вложений
    public List<Attachment> Attachments { get; set; } = new();
}