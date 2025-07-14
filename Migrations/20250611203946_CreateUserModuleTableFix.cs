using Microsoft.EntityFrameworkCore.Migrations;

namespace TeamTaskManager.Migrations
{
    public partial class CreateUserModuleTableFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserModuleAccesses_Users_UserId",
                table: "UserModuleAccesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserModuleAccesses",
                table: "UserModuleAccesses");

            migrationBuilder.RenameTable(
                name: "UserModuleAccesses",
                newName: "UserModules");

            migrationBuilder.RenameIndex(
                name: "IX_UserModuleAccesses_UserId",
                table: "UserModules",
                newName: "IX_UserModules_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserModules",
                table: "UserModules",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserModules_Users_UserId",
                table: "UserModules",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserModules_Users_UserId",
                table: "UserModules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserModules",
                table: "UserModules");

            migrationBuilder.RenameTable(
                name: "UserModules",
                newName: "UserModuleAccesses");

            migrationBuilder.RenameIndex(
                name: "IX_UserModules_UserId",
                table: "UserModuleAccesses",
                newName: "IX_UserModuleAccesses_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserModuleAccesses",
                table: "UserModuleAccesses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserModuleAccesses_Users_UserId",
                table: "UserModuleAccesses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
