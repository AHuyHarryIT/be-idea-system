using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class UpdateReviewStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Ideas");

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "Ideas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "Ideas");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Ideas",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
