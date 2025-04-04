using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyntheticArbitrage.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BinanceQBQDiffPrices",
                columns: table => new
                {
                    QuarterSymbol = table.Column<string>(type: "text", nullable: false),
                    BiQuarterSymbol = table.Column<string>(type: "text", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuarterPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    BiQuarterPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceDiff = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BinanceQBQDiffPrices", x => new { x.QuarterSymbol, x.BiQuarterSymbol, x.TimestampUtc });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BinanceQBQDiffPrices");
        }
    }
}
