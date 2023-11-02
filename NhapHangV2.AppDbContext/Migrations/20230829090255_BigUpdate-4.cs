using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhapHangV2.AppDbContext.Migrations
{
    public partial class BigUpdate4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailContent",
                table: "NotificationSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailSubject",
                table: "NotificationSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailAccountant",
                table: "NotificationSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailOrderer",
                table: "NotificationSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailSaler",
                table: "NotificationSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailStorekeepers",
                table: "NotificationSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailWarehoue",
                table: "NotificationSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailWarehoueFrom",
                table: "NotificationSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManagerContent",
                table: "NotificationSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerEmailContent",
                table: "NotificationSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerEmailSubject",
                table: "NotificationSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerUrl",
                table: "NotificationSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "NotificationSetting",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserContent",
                table: "NotificationSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserUrl",
                table: "NotificationSetting",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailContent",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "EmailSubject",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "IsEmailAccountant",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "IsEmailOrderer",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "IsEmailSaler",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "IsEmailStorekeepers",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "IsEmailWarehoue",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "IsEmailWarehoueFrom",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "ManagerContent",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "ManagerEmailContent",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "ManagerEmailSubject",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "ManagerUrl",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "UserContent",
                table: "NotificationSetting");

            migrationBuilder.DropColumn(
                name: "UserUrl",
                table: "NotificationSetting");
        }
    }
}
