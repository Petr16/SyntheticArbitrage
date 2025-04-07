using Microsoft.EntityFrameworkCore;
using SyntheticArbitrage.DAL.Entities;

namespace SyntheticArbitrage.DAL;

public class BinanceDbContext : DbContext
{
    public DbSet<BinanceQBQDiffPrice> BinanceQBQDiffPrices { get; set; }

    public BinanceDbContext(DbContextOptions<BinanceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BinanceQBQDiffPrice>()
            .HasKey(p => new { p.QuarterSymbol, p.BiQuarterSymbol, p.TimestampUtc, p.Interval });

        modelBuilder.Entity<BinanceQBQDiffPrice>()
        .HasIndex(p => new { p.QuarterSymbol, p.BiQuarterSymbol });
    }
}