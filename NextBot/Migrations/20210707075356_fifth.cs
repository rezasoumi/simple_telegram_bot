using Microsoft.EntityFrameworkCore.Migrations;

namespace NextBot.Migrations
{
    public partial class fifth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Save",
                table: "SmartPortfolioSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Save",
                table: "SmartPortfolioSetting");

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
        }
    }
}
