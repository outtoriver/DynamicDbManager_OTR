using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicDbManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TableDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemaName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColumnDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    Precision = table.Column<int>(type: "int", nullable: true),
                    Scale = table.Column<int>(type: "int", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryKey = table.Column<bool>(type: "bit", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColumnDefinitions_TableDefinitions_TableId",
                        column: x => x.TableId,
                        principalTable: "TableDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Relationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ParentTableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentColumnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferencedTableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferencedColumnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OnDelete = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OnUpdate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relationships_ColumnDefinitions_ParentColumnId",
                        column: x => x.ParentColumnId,
                        principalTable: "ColumnDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relationships_ColumnDefinitions_ReferencedColumnId",
                        column: x => x.ReferencedColumnId,
                        principalTable: "ColumnDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relationships_TableDefinitions_ParentTableId",
                        column: x => x.ParentTableId,
                        principalTable: "TableDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relationships_TableDefinitions_ReferencedTableId",
                        column: x => x.ReferencedTableId,
                        principalTable: "TableDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ColumnDefinitions_TableId_ColumnName",
                table: "ColumnDefinitions",
                columns: new[] { "TableId", "ColumnName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ParentColumnId",
                table: "Relationships",
                column: "ParentColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ParentTableId",
                table: "Relationships",
                column: "ParentTableId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ReferencedColumnId",
                table: "Relationships",
                column: "ReferencedColumnId");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ReferencedTableId",
                table: "Relationships",
                column: "ReferencedTableId");

            migrationBuilder.CreateIndex(
                name: "IX_TableDefinitions_SchemaName_TableName",
                table: "TableDefinitions",
                columns: new[] { "SchemaName", "TableName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileAttachments");

            migrationBuilder.DropTable(
                name: "Relationships");

            migrationBuilder.DropTable(
                name: "ColumnDefinitions");

            migrationBuilder.DropTable(
                name: "TableDefinitions");
        }
    }
}
