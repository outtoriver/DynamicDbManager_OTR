using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicDbManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentsAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_AdminTableRows_RowId",
                        column: x => x.RowId,
                        principalTable: "AdminTableRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTablePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    AdminTableId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTablePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTablePermissions_AdminTables_AdminTableId",
                        column: x => x.AdminTableId,
                        principalTable: "AdminTables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserTablePermissions_AdminTables_TableId",
                        column: x => x.TableId,
                        principalTable: "AdminTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTablePermissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_RowId",
                table: "Attachments",
                column: "RowId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTablePermissions_AdminTableId",
                table: "UserTablePermissions",
                column: "AdminTableId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTablePermissions_TableId",
                table: "UserTablePermissions",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTablePermissions_UserId_TableId",
                table: "UserTablePermissions",
                columns: new[] { "UserId", "TableId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "UserTablePermissions");
        }
    }
}
