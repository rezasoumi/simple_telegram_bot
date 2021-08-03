using Microsoft.EntityFrameworkCore.Migrations;

namespace NextBot.Migrations
{
    public partial class fifteenth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<double>(
                name: "MaximumStockWeight",
                table: "People",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinimumStockWeight",
                table: "People",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ProductionDate",
                table: "People",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RiskRate",
                table: "People",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "Save",
                table: "People",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumStockWeight",
                table: "People");

            migrationBuilder.DropColumn(
                name: "MinimumStockWeight",
                table: "People");

            migrationBuilder.DropColumn(
                name: "ProductionDate",
                table: "People");

            migrationBuilder.DropColumn(
                name: "RiskRate",
                table: "People");

            migrationBuilder.DropColumn(
                name: "Save",
                table: "People");

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
                    MaximumStockWeight = table.Column<double>(type: "float", nullable: false),
                    MinimumStockWeight = table.Column<double>(type: "float", nullable: false),
                    ProductionDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskRate = table.Column<long>(type: "bigint", nullable: false),
                    Save = table.Column<bool>(type: "bit", nullable: false)
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
    }
}
