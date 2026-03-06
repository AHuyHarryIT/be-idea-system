using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYear = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClousureDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FinalClousureDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdeaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailOutBoxId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailOutBoxes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailTo = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: false),
                    IdeaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOutBoxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailOutBoxes_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IdeaDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredPath = table.Column<string>(type: "text", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    FizeSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadtedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IdeaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeaDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdeaReactions",
                columns: table => new
                {
                    IdeaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reaction = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeaReactions", x => new { x.UserId, x.IdeaId });
                });

            migrationBuilder.CreateTable(
                name: "Ideas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailOutBoxId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdeaDocumentsId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdeaReactionsIdeaId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdeaReactionsUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ideas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ideas_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ideas_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ideas_EmailOutBoxes_EmailOutBoxId",
                        column: x => x.EmailOutBoxId,
                        principalTable: "EmailOutBoxes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ideas_IdeaDocuments_IdeaDocumentsId",
                        column: x => x.IdeaDocumentsId,
                        principalTable: "IdeaDocuments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ideas_IdeaReactions_IdeaReactionsUserId_IdeaReactionsIdeaId",
                        columns: x => new { x.IdeaReactionsUserId, x.IdeaReactionsIdeaId },
                        principalTable: "IdeaReactions",
                        principalColumns: new[] { "UserId", "IdeaId" });
                    table.ForeignKey(
                        name: "FK_Ideas_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    HashPassword = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Avartar = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdeaReactionsIdeaId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdeaReactionsUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_IdeaReactions_IdeaReactionsUserId_IdeaReactionsIdeaId",
                        columns: x => new { x.IdeaReactionsUserId, x.IdeaReactionsIdeaId },
                        principalTable: "IdeaReactions",
                        principalColumns: new[] { "UserId", "IdeaId" });
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_EmailOutBoxId",
                table: "Comments",
                column: "EmailOutBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IdeaId",
                table: "Comments",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_UserId",
                table: "Departments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutBoxes_CommentId",
                table: "EmailOutBoxes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutBoxes_IdeaId",
                table: "EmailOutBoxes",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_IdeaDocuments_IdeaId",
                table: "IdeaDocuments",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_IdeaReactions_IdeaId",
                table: "IdeaReactions",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_CategoryId",
                table: "Ideas",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_DepartmentId",
                table: "Ideas",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_EmailOutBoxId",
                table: "Ideas",
                column: "EmailOutBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_IdeaDocumentsId",
                table: "Ideas",
                column: "IdeaDocumentsId");

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Ideas",
                columns: new[] { "IdeaReactionsUserId", "IdeaReactionsIdeaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_SubmissionId",
                table: "Ideas",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Ideas_UserId",
                table: "Ideas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UserId",
                table: "Roles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdeaReactionsUserId_IdeaReactionsIdeaId",
                table: "Users",
                columns: new[] { "IdeaReactionsUserId", "IdeaReactionsIdeaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_EmailOutBoxes_EmailOutBoxId",
                table: "Comments",
                column: "EmailOutBoxId",
                principalTable: "EmailOutBoxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Ideas_IdeaId",
                table: "Comments",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_UserId",
                table: "Departments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailOutBoxes_Ideas_IdeaId",
                table: "EmailOutBoxes",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IdeaDocuments_Ideas_IdeaId",
                table: "IdeaDocuments",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdeaReactions_Ideas_IdeaId",
                table: "IdeaReactions",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdeaReactions_Users_UserId",
                table: "IdeaReactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Users_UserId",
                table: "Ideas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Users_UserId",
                table: "Roles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_EmailOutBoxes_EmailOutBoxId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_EmailOutBoxes_EmailOutBoxId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_IdeaDocuments_Ideas_IdeaId",
                table: "IdeaDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_IdeaReactions_Ideas_IdeaId",
                table: "IdeaReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_UserId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_IdeaReactions_Users_UserId",
                table: "IdeaReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Users_UserId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "EmailOutBoxes");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Ideas");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "IdeaDocuments");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "IdeaReactions");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
