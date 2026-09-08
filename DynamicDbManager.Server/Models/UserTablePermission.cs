namespace DynamicDbManager.Server.Models;

public class UserTablePermission
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int TableId { get; set; }

    public bool CanView { get; set; } = true;   // просмотр

    public bool CanEdit { get; set; } = false;  // создание и изменение записей

    public bool CanDelete { get; set; } = false; // удаление записей и вложений

    public ApplicationUser User { get; set; } = null!;
    public AdminTable Table { get; set; } = null!;
}
