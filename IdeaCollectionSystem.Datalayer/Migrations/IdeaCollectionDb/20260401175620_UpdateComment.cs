using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class UpdateComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DELETE FROM \"Comments\"");
			migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_IdeaUser_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "IdeaUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_IdeaUser_UserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId",
                table: "Comments");
        }
    }
}
