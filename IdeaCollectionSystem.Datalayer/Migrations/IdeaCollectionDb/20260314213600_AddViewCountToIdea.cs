using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class AddViewCountToIdea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Ideas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Ideas");
        }
    }
}
