using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class AddIdeaReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdeaReactionsIdeaId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaReactionsUserId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaReactionsIdeaId",
                table: "Ideas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaReactionsUserId",
                table: "Ideas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IdeaReactions",
                columns: table => new
                {
                    IdeaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reaction = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeaReactions", x => new { x.UserId, x.IdeaId });
                    table.ForeignKey(
                        name: "FK_IdeaReactions_Ideas_IdeaId",
                        column: x => x.IdeaId,
                        principalTable: "Ideas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IdeaReactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Users",
                columns: new[] { "IdeaReactionsUserId", "IdeaReactionsIdeaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Ideas",
                columns: new[] { "IdeaReactionsUserId", "IdeaReactionsIdeaId" });

            migrationBuilder.CreateIndex(
                name: "IX_IdeaReactions_IdeaId",
                table: "IdeaReactions",
                column: "IdeaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_IdeaReactions_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Ideas",
                columns: new[] { "IdeaReactionsUserId", "IdeaReactionsIdeaId" },
                principalTable: "IdeaReactions",
                principalColumns: new[] { "UserId", "IdeaId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Users_IdeaReactions_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Users",
                columns: new[] { "IdeaReactionsUserId", "IdeaReactionsIdeaId" },
                principalTable: "IdeaReactions",
                principalColumns: new[] { "UserId", "IdeaId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_IdeaReactions_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_IdeaReactions_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "IdeaReactions");

            migrationBuilder.DropIndex(
                name: "IX_Users_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Ideas_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Ideas");

            migrationBuilder.DropColumn(
                name: "IdeaReactionsIdeaId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdeaReactionsUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdeaReactionsIdeaId",
                table: "Ideas");

            migrationBuilder.DropColumn(
                name: "IdeaReactionsUserId",
                table: "Ideas");
        }
    }
}
