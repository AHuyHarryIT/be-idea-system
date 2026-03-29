using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class FixAcademicYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "Submissions",
                type: "integer",
                nullable: false,
		        defaultValue: 2026); 
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcademicYear",
                table: "Submissions",
                type: "timestamp without time zone",
			    nullable: false, 
		        defaultValue: 2026); 
		}
    }
}
