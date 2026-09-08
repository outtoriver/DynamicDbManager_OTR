using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable

namespace DynamicDbManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class SecComp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTablePermissions_AdminTables_AdminTableId",
                table: "UserTablePermissions");
            migrationBuilder.DropIndex(
                name: "IX_UserTablePermissions_AdminTableId",
                table: "UserTablePermissions");

            migrationBuilder.DropColumn(
                name: "AdminTableId",
                table: "UserTablePermissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminTableId",
                table: "UserTablePermissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTablePermissions_AdminTableId",
                table: "UserTablePermissions",
                column: "AdminTableId");
            migrationBuilder.AddForeignKey(
                name: "FK_UserTablePermissions_AdminTables_AdminTableId",
                table: "UserTablePermissions",
                column: "AdminTableId",
                principalTable: "AdminTables",
                principalColumn: "Id");
        }
    }
}
