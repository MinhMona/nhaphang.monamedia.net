using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhapHangV2.AppDbContext.Migrations
{
    public partial class BigUpdate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualShippingCost",
                table: "TransportationOrder",
                type: "decimal(18,0)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDate",
                table: "TransportationOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ComplainDate",
                table: "TransportationOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompleteDate",
                table: "TransportationOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmDate",
                table: "TransportationOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidDate",
                table: "TransportationOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TQDate",
                table: "TransportationOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VNDate",
                table: "TransportationOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ComplainDate",
                table: "MainOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportationOrderId",
                table: "Complain",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualShippingCost",
                table: "TransportationOrder");

            migrationBuilder.DropColumn(
                name: "CancelDate",
                table: "TransportationOrder");

            migrationBuilder.DropColumn(
                name: "ComplainDate",
                table: "TransportationOrder");

            migrationBuilder.DropColumn(
                name: "CompleteDate",
                table: "TransportationOrder");

            migrationBuilder.DropColumn(
                name: "ConfirmDate",
                table: "TransportationOrder");

            migrationBuilder.DropColumn(
                name: "PaidDate",
                table: "TransportationOrder");

            migrationBuilder.DropColumn(
                name: "TQDate",
                table: "TransportationOrder");

            migrationBuilder.DropColumn(
                name: "VNDate",
                table: "TransportationOrder");

            migrationBuilder.DropColumn(
                name: "ComplainDate",
                table: "MainOrder");

            migrationBuilder.DropColumn(
                name: "TransportationOrderId",
                table: "Complain");
        }
    }
}
