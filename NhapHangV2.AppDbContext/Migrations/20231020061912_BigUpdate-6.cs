using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhapHangV2.AppDbContext.Migrations
{
    public partial class BigUpdate6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FeeBuyProPercent",
                table: "OrderShopTemp",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EditedFeeBuyProPercent",
                table: "MainOrder",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEditFeeBuyProPercent",
                table: "MainOrder",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeBuyProPercent",
                table: "OrderShopTemp");

            migrationBuilder.DropColumn(
                name: "EditedFeeBuyProPercent",
                table: "MainOrder");

            migrationBuilder.DropColumn(
                name: "IsEditFeeBuyProPercent",
                table: "MainOrder");
        }
    }
}
