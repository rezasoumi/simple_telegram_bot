﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NextBot.Models;

namespace NextBot.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20210710105610_sixth")]
    partial class sixth
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("NextBot.Models.Person", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("ChatId")
                        .HasColumnType("bigint");

                    b.Property<long>("ClassicNextSelectState")
                        .HasColumnType("bigint");

                    b.Property<bool>("GetDate")
                        .HasColumnType("bit");

                    b.Property<bool>("GetMaximumStockWeight")
                        .HasColumnType("bit");

                    b.Property<bool>("GetMinimumStockWeight")
                        .HasColumnType("bit");

                    b.Property<bool>("GetRisk")
                        .HasColumnType("bit");

                    b.Property<bool>("GetSave")
                        .HasColumnType("bit");

                    b.Property<long>("PorfolioIdForClassicNextSelect")
                        .HasColumnType("bigint");

                    b.Property<long?>("SmartPortfolioSettingId")
                        .HasColumnType("bigint");

                    b.Property<long>("State")
                        .HasColumnType("bigint");

                    b.Property<int>("TickerKeyForStock")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SmartPortfolioSettingId");

                    b.ToTable("People");
                });

            modelBuilder.Entity("NextBot.Models.SmartPortfolioSetting", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("MaximumStockWeight")
                        .HasColumnType("float");

                    b.Property<double>("MinimumStockWeight")
                        .HasColumnType("float");

                    b.Property<string>("ProductionDate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("RiskRate")
                        .HasColumnType("bigint");

                    b.Property<bool>("Save")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("SmartPortfolioSetting");
                });

            modelBuilder.Entity("NextBot.Models.Person", b =>
                {
                    b.HasOne("NextBot.Models.SmartPortfolioSetting", "SmartPortfolioSetting")
                        .WithMany()
                        .HasForeignKey("SmartPortfolioSettingId");

                    b.Navigation("SmartPortfolioSetting");
                });
#pragma warning restore 612, 618
        }
    }
}
