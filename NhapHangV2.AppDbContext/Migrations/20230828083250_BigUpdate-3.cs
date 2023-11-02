using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhapHangV2.AppDbContext.Migrations
{
    public partial class BigUpdate3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ComingVNDate",
                table: "TransportationOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOutTQ",
                table: "SmallPackage",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffOutTQ",
                table: "SmallPackage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateComingVN",
                table: "MainOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSendGoods",
                table: "MainOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportationOrderId",
                table: "HistoryOrderChange",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComingVNDate",
                table: "TransportationOrder");

            migrationBuilder.DropColumn(
                name: "DateOutTQ",
                table: "SmallPackage");

            migrationBuilder.DropColumn(
                name: "StaffOutTQ",
                table: "SmallPackage");

            migrationBuilder.DropColumn(
                name: "DateComingVN",
                table: "MainOrder");

            migrationBuilder.DropColumn(
                name: "DateSendGoods",
                table: "MainOrder");

            migrationBuilder.DropColumn(
                name: "TransportationOrderId",
                table: "HistoryOrderChange");
        }
    }
}
