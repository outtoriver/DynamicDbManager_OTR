using System.ComponentModel.DataAnnotations;

namespace DynamicDbManager.Server.Models;

public class AdminTable
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string TableColumnsJson { get; set; } = "[]";
    public string ModalColumnsJson { get; set; } = "[]";

    // Обратная навигация для той же связи, которая использует
    // UserTablePermission.TableId.
    public List<UserTablePermission> Permissions { get; set; } = new();

    public List<AdminTableRow> Rows { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TableColumn
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
}

public class ModalColumn
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
}