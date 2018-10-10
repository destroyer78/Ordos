using Microsoft.EntityFrameworkCore.Migrations;

namespace Ordos.DataService.Migrations
{
    public partial class AddBayId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BayId",
                table: "Devices",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BayId",
                table: "Devices");
        }
    }
}
