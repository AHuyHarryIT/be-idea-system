using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class ChangeTextToTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Ideas",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Comments",
                newName: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Ideas",
                newName: "Text");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Comments",
                newName: "Text");
        }
    }
}
