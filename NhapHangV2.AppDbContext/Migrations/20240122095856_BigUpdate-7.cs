using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhapHangV2.AppDbContext.Migrations
{
    public partial class BigUpdate7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAppAccepted",
                table: "Configurations",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAppAccepted",
                table: "Configurations");
        }
    }
}
