using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicDbManager.Server.Migrations;

/// <inheritdoc />
public partial class FixAdminTableRowRelation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Older versions accidentally introduced a nullable shadow AdminTableId.
        // The application has always treated TableId as the real relationship.
        // Preserve any usable legacy relationship before removing the shadow column.
        migrationBuilder.Sql(@"
UPDATE r
SET r.TableId = r.AdminTableId
FROM AdminTableRows r
LEFT JOIN AdminTables t ON t.Id = r.TableId
WHERE r.AdminTableId IS NOT NULL
  AND t.Id IS NULL;");

        migrationBuilder.DropForeignKey(
            name: "FK_AdminTableRows_AdminTables_AdminTableId",
            table: "AdminTableRows");

        migrationBuilder.DropIndex(
            name: "IX_AdminTableRows_AdminTableId",
            table: "AdminTableRows");

        migrationBuilder.DropColumn(
            name: "AdminTableId",
            table: "AdminTableRows");

        migrationBuilder.CreateIndex(
            name: "IX_AdminTableRows_TableId",
            table: "AdminTableRows",
            column: "TableId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AdminTableRows_TableId",
            table: "AdminTableRows");

        migrationBuilder.AddColumn<int>(
            name: "AdminTableId",
            table: "AdminTableRows",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AdminTableRows_AdminTableId",
            table: "AdminTableRows",
            column: "AdminTableId");

        migrationBuilder.AddForeignKey(
            name: "FK_AdminTableRows_AdminTables_AdminTableId",
            table: "AdminTableRows",
            column: "AdminTableId",
            principalTable: "AdminTables",
            principalColumn: "Id");
    }
}
