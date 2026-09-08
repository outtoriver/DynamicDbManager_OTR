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

        // AdminTable -> AdminTableRow использует реальный TableId.
        // ClientCascade намеренно не создаёт новый SQL FK: старые базы могли
        // содержать неконсистентные строки, и мы не хотим блокировать миграцию.
        // При удалении таблицы контроллер удаляет загруженные строки явно.
        modelBuilder.Entity<AdminTableRow>()
            .HasOne(r => r.Table)
            .WithMany(t => t.Rows)
            .HasForeignKey(r => r.TableId)
            .OnDelete(DeleteBehavior.ClientCascade);

        modelBuilder.Entity<AdminTableRow>()
            .HasIndex(r => r.TableId);

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

        modelBuilder.Entity<UserTablePermission>()
            .HasOne(p => p.Table)
            .WithMany(t => t.Permissions)
            .HasForeignKey(p => p.TableId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTablePermission>()
            .HasIndex(p => new { p.UserId, p.TableId })
            .IsUnique();
    }
}
