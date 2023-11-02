using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhapHangV2.AppDbContext.Migrations
{
    public partial class BigUpdate1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminImage",
                table: "PayHelpDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserImage",
                table: "PayHelpDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDate",
                table: "PayHelp",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompleteDate",
                table: "PayHelp",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmDate",
                table: "PayHelp",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeService",
                table: "PayHelp",
                type: "decimal(18,0)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidDate",
                table: "PayHelp",
                type: "datetime2",
                nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminImage",
                table: "PayHelpDetail");

            migrationBuilder.DropColumn(
                name: "UserImage",
                table: "PayHelpDetail");

            migrationBuilder.DropColumn(
                name: "CancelDate",
                table: "PayHelp");

            migrationBuilder.DropColumn(
                name: "CompleteDate",
                table: "PayHelp");

            migrationBuilder.DropColumn(
                name: "ConfirmDate",
                table: "PayHelp");

            migrationBuilder.DropColumn(
                name: "FeeService",
                table: "PayHelp");

            migrationBuilder.DropColumn(
                name: "PaidDate",
                table: "PayHelp");

        }
    }
}
