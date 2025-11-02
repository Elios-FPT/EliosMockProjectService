using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MockProjectService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StepNumber",
                table: "Processes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StepNumber",
                table: "Processes");
        }
    }
}
