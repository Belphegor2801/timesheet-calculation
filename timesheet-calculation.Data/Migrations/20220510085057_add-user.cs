using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace timesheet_calculation.Data.Migrations
{
    public partial class adduser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "im_User",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_im_User", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_im_TimeSheet_UserId",
                table: "im_TimeSheet",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_im_TimeSheet_im_User_UserId",
                table: "im_TimeSheet",
                column: "UserId",
                principalTable: "im_User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_im_TimeSheet_im_User_UserId",
                table: "im_TimeSheet");

            migrationBuilder.DropTable(
                name: "im_User");

            migrationBuilder.DropIndex(
                name: "IX_im_TimeSheet_UserId",
                table: "im_TimeSheet");
        }
    }
}
