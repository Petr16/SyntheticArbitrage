using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyntheticArbitrage.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToBinanceQBQDiffPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BinanceQBQDiffPrices_QuarterSymbol_BiQuarterSymbol",
                table: "BinanceQBQDiffPrices",
                columns: new[] { "QuarterSymbol", "BiQuarterSymbol" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BinanceQBQDiffPrices_QuarterSymbol_BiQuarterSymbol",
                table: "BinanceQBQDiffPrices");
        }
    }
}
