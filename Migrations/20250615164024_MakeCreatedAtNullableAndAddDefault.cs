using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace TeamTaskManager.Migrations
{
    public partial class MakeCreatedAtNullableAndAddDefault : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime?>(
                name: "CreatedAt",
                table: "Projects",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETDATE()",  // 👈 این خط رو اضافه کن
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
               name: "CreatedAt",
               table: "Projects",
               type: "datetime2",
               nullable: false,
               oldClrType: typeof(DateTime?),
               oldType: "datetime2",
               oldDefaultValueSql: "GETDATE()");
        }
    }
}
