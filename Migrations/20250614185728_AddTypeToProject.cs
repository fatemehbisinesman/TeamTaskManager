using Microsoft.EntityFrameworkCore.Migrations;

namespace TeamTaskManager.Migrations
{
    public partial class AddTypeToProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserModules_ProjectId",
                table: "UserModules",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserModules_Projects_ProjectId",
                table: "UserModules",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserModules_Projects_ProjectId",
                table: "UserModules");

            migrationBuilder.DropIndex(
                name: "IX_UserModules_ProjectId",
                table: "UserModules");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Projects");
        }
    }
}
