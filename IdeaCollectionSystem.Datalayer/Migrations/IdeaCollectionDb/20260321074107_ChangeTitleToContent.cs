using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class ChangeTitleToContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadtedAt",
                table: "IdeaDocuments",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "FizeSize",
                table: "IdeaDocuments",
                newName: "FileSize");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Comments",
                newName: "Content");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "IdeaDocuments",
                newName: "UploadtedAt");

            migrationBuilder.RenameColumn(
                name: "FileSize",
                table: "IdeaDocuments",
                newName: "FizeSize");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Comments",
                newName: "Title");
        }
    }
}
