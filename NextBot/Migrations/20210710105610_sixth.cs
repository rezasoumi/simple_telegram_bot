using Microsoft.EntityFrameworkCore.Migrations;

namespace NextBot.Migrations
{
    public partial class sixth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TickerKeyForStock",
                table: "People",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TickerKeyForStock",
                table: "People");
        }
    }
}
