using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECDBugTracker.data.Migrations
{
    public partial class _004postGresDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Project");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Project",
                type: "text",
                nullable: true);
        }
    }
}
