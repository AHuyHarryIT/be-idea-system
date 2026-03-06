using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class AddDbIdeaDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Ideas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaDocumentsId",
                table: "Ideas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IdeaDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredPath = table.Column<string>(type: "text", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    FizeSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadtedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdeaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeaDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdeaDocuments_Ideas_IdeaId",
                        column: x => x.IdeaId,
                        principalTable: "Ideas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_IdeaDocumentsId",
                table: "Ideas",
                column: "IdeaDocumentsId");

            migrationBuilder.CreateIndex(
                name: "IX_IdeaDocuments_IdeaId",
                table: "IdeaDocuments",
                column: "IdeaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_IdeaDocuments_IdeaDocumentsId",
                table: "Ideas",
                column: "IdeaDocumentsId",
                principalTable: "IdeaDocuments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_IdeaDocuments_IdeaDocumentsId",
                table: "Ideas");

            migrationBuilder.DropTable(
                name: "IdeaDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Ideas_IdeaDocumentsId",
                table: "Ideas");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Ideas");

            migrationBuilder.DropColumn(
                name: "IdeaDocumentsId",
                table: "Ideas");
        }
    }
}
