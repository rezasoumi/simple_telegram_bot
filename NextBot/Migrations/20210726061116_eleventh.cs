using Microsoft.EntityFrameworkCore.Migrations;

namespace NextBot.Migrations
{
    public partial class eleventh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreateSmartPortfolioType",
                table: "People",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateSmartPortfolioType",
                table: "People");
        }
    }
}
