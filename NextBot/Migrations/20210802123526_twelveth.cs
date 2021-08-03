using Microsoft.EntityFrameworkCore.Migrations;

namespace NextBot.Migrations
{
    public partial class twelveth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GetDate",
                table: "People");

            migrationBuilder.DropColumn(
                name: "GetMaximumStockWeight",
                table: "People");

            migrationBuilder.DropColumn(
                name: "GetMinimumStockWeight",
                table: "People");

            migrationBuilder.DropColumn(
                name: "GetRisk",
                table: "People");

            migrationBuilder.DropColumn(
                name: "GetSave",
                table: "People");

            migrationBuilder.DropColumn(
                name: "State",
                table: "People");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GetDate",
                table: "People",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GetMaximumStockWeight",
                table: "People",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GetMinimumStockWeight",
                table: "People",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GetRisk",
                table: "People",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GetSave",
                table: "People",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "State",
                table: "People",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
