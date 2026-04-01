using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class AddIdeaViewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdeaViews",
                table: "IdeaViews");

            migrationBuilder.RenameColumn(
                name: "VistiTime",
                table: "IdeaViews",
                newName: "ViewedAt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "IdeaViews",
                newName: "IdeaId");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "IdeaViews",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdeaViews",
                table: "IdeaViews",
                columns: new[] { "IdeaId", "UserId" });

            migrationBuilder.CreateTable(
                name: "IdeaUser",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeaUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdeaUser_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdeaViews_UserId",
                table: "IdeaViews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IdeaUser_DepartmentId",
                table: "IdeaUser",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_IdeaViews_IdeaUser_UserId",
                table: "IdeaViews",
                column: "UserId",
                principalTable: "IdeaUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdeaViews_Ideas_IdeaId",
                table: "IdeaViews",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdeaViews_IdeaUser_UserId",
                table: "IdeaViews");

            migrationBuilder.DropForeignKey(
                name: "FK_IdeaViews_Ideas_IdeaId",
                table: "IdeaViews");

            migrationBuilder.DropTable(
                name: "IdeaUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdeaViews",
                table: "IdeaViews");

            migrationBuilder.DropIndex(
                name: "IX_IdeaViews_UserId",
                table: "IdeaViews");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "IdeaViews");

            migrationBuilder.RenameColumn(
                name: "ViewedAt",
                table: "IdeaViews",
                newName: "VistiTime");

            migrationBuilder.RenameColumn(
                name: "IdeaId",
                table: "IdeaViews",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdeaViews",
                table: "IdeaViews",
                column: "Id");
        }
    }
}
