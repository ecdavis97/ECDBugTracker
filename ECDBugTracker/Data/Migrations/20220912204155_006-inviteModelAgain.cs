using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECDBugTracker.data.Migrations
{
    public partial class _006inviteModelAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Project_ProjectId",
                table: "Invites");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Invites",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Project_ProjectId",
                table: "Invites",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invites_Project_ProjectId",
                table: "Invites");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Invites",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Invites_Project_ProjectId",
                table: "Invites",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id");
        }
    }
}
