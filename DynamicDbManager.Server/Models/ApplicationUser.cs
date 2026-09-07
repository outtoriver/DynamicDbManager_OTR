using Microsoft.AspNetCore.Identity;

namespace DynamicDbManager.Server.Models;

public class ApplicationUser : IdentityUser
{
    // Навигационные свойства
    public List<UserTablePermission> TablePermissions { get; set; } = new();
}