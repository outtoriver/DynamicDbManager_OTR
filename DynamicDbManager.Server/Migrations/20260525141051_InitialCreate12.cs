using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicDbManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminTableRows_AdminTables_TableId",
                table: "AdminTableRows");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddForeignKey(
                name: "FK_AdminTableRows_AdminTables_TableId",
                table: "AdminTableRows",
                column: "TableId",
                principalTable: "AdminTables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
