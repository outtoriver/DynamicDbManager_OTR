namespace DynamicDbManager.Server.Models;

public class UserTablePermission
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int TableId { get; set; }

    public bool CanView { get; set; } = true;   // просмотр

    public bool CanEdit { get; set; } = false;  // редактирование (создание/изменение/удаление записей)

    public bool CanDelete { get; set; } = false; // удаление таблицы (только для админов?)

    // Навигация
    public ApplicationUser User { get; set; } = null!;
    public AdminTable Table { get; set; } = null!;
}