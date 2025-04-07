﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SyntheticArbitrage.DAL;

#nullable disable

namespace SyntheticArbitrage.DAL.Migrations
{
    [DbContext(typeof(BinanceDbContext))]
    partial class BinanceDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SyntheticArbitrage.DAL.Entities.BinanceQBQDiffPrice", b =>
                {
                    b.Property<string>("QuarterSymbol")
                        .HasColumnType("text");

                    b.Property<string>("BiQuarterSymbol")
                        .HasColumnType("text");

                    b.Property<DateTime>("TimestampUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Interval")
                        .HasColumnType("integer");

                    b.Property<decimal>("BiQuarterPrice")
                        .HasColumnType("numeric");

                    b.Property<decimal>("PriceDiff")
                        .HasColumnType("numeric");

                    b.Property<decimal>("QuarterPrice")
                        .HasColumnType("numeric");

                    b.HasKey("QuarterSymbol", "BiQuarterSymbol", "TimestampUtc", "Interval");

                    b.HasIndex("QuarterSymbol", "BiQuarterSymbol");

                    b.ToTable("BinanceQBQDiffPrices");
                });
#pragma warning restore 612, 618
        }
    }
}
