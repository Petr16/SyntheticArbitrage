using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyntheticArbitrage.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIntervalToBinanceQBQDifPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BinanceQBQDiffPrices",
                table: "BinanceQBQDiffPrices");

            migrationBuilder.AddColumn<int>(
                name: "Interval",
                table: "BinanceQBQDiffPrices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BinanceQBQDiffPrices",
                table: "BinanceQBQDiffPrices",
                columns: new[] { "QuarterSymbol", "BiQuarterSymbol", "TimestampUtc", "Interval" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BinanceQBQDiffPrices",
                table: "BinanceQBQDiffPrices");

            migrationBuilder.DropColumn(
                name: "Interval",
                table: "BinanceQBQDiffPrices");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BinanceQBQDiffPrices",
                table: "BinanceQBQDiffPrices",
                columns: new[] { "QuarterSymbol", "BiQuarterSymbol", "TimestampUtc" });
        }
    }
}
