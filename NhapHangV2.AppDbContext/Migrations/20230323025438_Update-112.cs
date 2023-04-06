using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhapHangV2.AppDbContext.Migrations
{
    public partial class Update112 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "PageType",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "PageType");
        }
    }
}
