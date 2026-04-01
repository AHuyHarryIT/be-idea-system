using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdeaCollectionSystem.Datalayer.Migrations.IdeaCollectionDb
{
    /// <inheritdoc />
    public partial class ForceCleanComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DELETE FROM \"Comments\"");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
