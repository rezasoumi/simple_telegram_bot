using Microsoft.EntityFrameworkCore.Migrations;

namespace NextBot.Migrations
{
    public partial class twice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SmartPortfolioSettingId",
                table: "People",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SmartPortfolioSetting",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RiskRate = table.Column<long>(type: "bigint", nullable: false),
                    MaximumStockWeight = table.Column<double>(type: "float", nullable: false),
                    MinimumStockWeight = table.Column<double>(type: "float", nullable: false),
                    ProductionDate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartPortfolioSetting", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_People_SmartPortfolioSettingId",
                table: "People",
                column: "SmartPortfolioSettingId");

            migrationBuilder.AddForeignKey(
                name: "FK_People_SmartPortfolioSetting_SmartPortfolioSettingId",
                table: "People",
                column: "SmartPortfolioSettingId",
                principalTable: "SmartPortfolioSetting",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_SmartPortfolioSetting_SmartPortfolioSettingId",
                table: "People");

            migrationBuilder.DropTable(
                name: "SmartPortfolioSetting");

            migrationBuilder.DropIndex(
                name: "IX_People_SmartPortfolioSettingId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "SmartPortfolioSettingId",
                table: "People");
        }
    }
}
