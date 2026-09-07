using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DynamicDbManager.Server.Models;

namespace DynamicDbManager.Server.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdminTable> AdminTables => Set<AdminTable>();
    public DbSet<AdminTableRow> AdminTableRows => Set<AdminTableRow>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<UserTablePermission> UserTablePermissions => Set<UserTablePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Attachment>()
            .HasOne(a => a.Row)
            .WithMany(r => r.Attachments)
            .HasForeignKey(a => a.RowId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTablePermission>()
            .HasOne(p => p.User)
            .WithMany(u => u.TablePermissions)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ОДНА связь Permission -> AdminTable.
        // Используем существующий TableId и существующую
        // навигацию AdminTable.Permissions.
        //
        // Это предотвращает создание EF Core скрытого
        // shadow-property AdminTableId.
        modelBuilder.Entity<UserTablePermission>()
            .HasOne(p => p.Table)
            .WithMany(t => t.Permissions)
            .HasForeignKey(p => p.TableId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один пользователь = одна запись прав для конкретной таблицы.
        modelBuilder.Entity<UserTablePermission>()
            .HasIndex(p => new { p.UserId, p.TableId })
            .IsUnique();
    }
}