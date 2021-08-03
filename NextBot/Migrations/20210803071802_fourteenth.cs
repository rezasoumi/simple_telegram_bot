using Microsoft.EntityFrameworkCore.Migrations;

namespace NextBot.Migrations
{
    public partial class fourteenth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmartPortfolioSetting_People_PersonId",
                table: "SmartPortfolioSetting");

            migrationBuilder.DropIndex(
                name: "IX_SmartPortfolioSetting_PersonId",
                table: "SmartPortfolioSetting");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "SmartPortfolioSetting");

            migrationBuilder.AddColumn<long>(
                name: "SmartPortfolioSettingId",
                table: "People",
                type: "bigint",
                nullable: true);

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

            migrationBuilder.DropIndex(
                name: "IX_People_SmartPortfolioSettingId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "SmartPortfolioSettingId",
                table: "People");

            migrationBuilder.AddColumn<long>(
                name: "PersonId",
                table: "SmartPortfolioSetting",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_SmartPortfolioSetting_PersonId",
                table: "SmartPortfolioSetting",
                column: "PersonId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SmartPortfolioSetting_People_PersonId",
                table: "SmartPortfolioSetting",
                column: "PersonId",
                principalTable: "People",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
