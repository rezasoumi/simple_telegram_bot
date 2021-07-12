using Microsoft.EntityFrameworkCore.Migrations;

namespace NextBot.Migrations
{
    public partial class seventh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PorfolioIdForClassicNextSelect",
                table: "People",
                newName: "PortfolioIdForClassicNextSelect");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PortfolioIdForClassicNextSelect",
                table: "People",
                newName: "PorfolioIdForClassicNextSelect");
        }
    }
}
