using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicDbManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class EditedRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModalColumnsJson",
                table: "TableDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModalColumnsJson",
                table: "TableDefinitions");
        }
    }
}
