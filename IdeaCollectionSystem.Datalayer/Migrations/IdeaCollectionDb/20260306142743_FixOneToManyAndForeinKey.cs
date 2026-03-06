using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class FixOneToManyAndForeinKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Ideas_IdeaId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Ideas_IdeaId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_EmailOutBoxes_Comments_CommentId",
                table: "EmailOutBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Categories_CategoryId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Departments_DepartmentId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Submissions_SubmissionId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Ideas_IdeaId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Ideas_IdeaId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IdeaId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_IdeaId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Departments_IdeaId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Categories_IdeaId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IdeaId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdeaId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "IdeaId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "IdeaId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "FinaleClosureDate",
                table: "Submissions",
                newName: "FinalClousureDate");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Avartar",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailOutBoxes_Comments_CommentId",
                table: "EmailOutBoxes",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Categories_CategoryId",
                table: "Ideas",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Departments_DepartmentId",
                table: "Ideas",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Submissions_SubmissionId",
                table: "Ideas",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailOutBoxes_Comments_CommentId",
                table: "EmailOutBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Categories_CategoryId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Departments_DepartmentId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Submissions_SubmissionId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "FinalClousureDate",
                table: "Submissions",
                newName: "FinaleClosureDate");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Avartar",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaId",
                table: "Submissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaId",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdeaId",
                table: "Categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdeaId",
                table: "Users",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_IdeaId",
                table: "Submissions",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IdeaId",
                table: "Departments",
                column: "IdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IdeaId",
                table: "Categories",
                column: "IdeaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Ideas_IdeaId",
                table: "Categories",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Ideas_IdeaId",
                table: "Departments",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailOutBoxes_Comments_CommentId",
                table: "EmailOutBoxes",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Categories_CategoryId",
                table: "Ideas",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Departments_DepartmentId",
                table: "Ideas",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Submissions_SubmissionId",
                table: "Ideas",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Ideas_IdeaId",
                table: "Submissions",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Ideas_IdeaId",
                table: "Users",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
