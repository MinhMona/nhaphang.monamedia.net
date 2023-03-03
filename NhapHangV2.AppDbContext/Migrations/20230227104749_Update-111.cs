using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhapHangV2.AppDbContext.Migrations
{
    public partial class Update111 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsChangeFeeWeight",
                table: "MainOrder",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsChangeTQVNWeight",
                table: "MainOrder",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChangeFeeWeight",
                table: "MainOrder");

            migrationBuilder.DropColumn(
                name: "IsChangeTQVNWeight",
                table: "MainOrder");
        }
    }
}
