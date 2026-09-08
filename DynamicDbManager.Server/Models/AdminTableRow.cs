namespace DynamicDbManager.Server.Models;

public class AdminTableRow
{
    public int Id { get; set; }

    public int TableId { get; set; }

    public string DataJson { get; set; } = "{}";

    // Явная связь по существующему TableId.
    // EF Core больше не должен создавать скрытый AdminTableId.
    public AdminTable Table { get; set; } = null!;

    public List<Attachment> Attachments { get; set; } = new();
}
