﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductsCRUD.Context;

namespace ProductsCRUD.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20220109125110_ProperMigration")]
    partial class ProperMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.11")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ProductsCRUD.DomainModels.ProductDomainModel", b =>
                {
                    b.Property<int>("ProductID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ProductDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("ProductPrice")
                        .HasColumnType("float");

                    b.Property<int>("ProductQuantity")
                        .HasColumnType("int");

                    b.HasKey("ProductID");

                    b.ToTable("_products");

                    b.HasData(
                        new
                        {
                            ProductID = 1,
                            ProductDescription = "For his or her sensory pleasure. Fits few known smartphones.",
                            ProductName = "Rippled Screen Protector",
                            ProductPrice = 8.2899999999999991,
                            ProductQuantity = 4
                        },
                        new
                        {
                            ProductID = 2,
                            ProductDescription = "Poor quality fake faux leather cover, loose enough to fit any mobile device.",
                            ProductName = "Wrap it and Hope Cover",
                            ProductPrice = 5.7800000000000002,
                            ProductQuantity = 45
                        },
                        new
                        {
                            ProductID = 3,
                            ProductDescription = "Purchase your favourite chocolate and use the provided heating element t melt it into the perfect cover for your phone.",
                            ProductName = "Chocolate Cover",
                            ProductPrice = 11.82,
                            ProductQuantity = 1243
                        },
                        new
                        {
                            ProductID = 4,
                            ProductDescription = "Place your device within the water-tight container, fill with water and enjoy the cushioned protection from bumps and bangs.",
                            ProductName = "Water Bath Case",
                            ProductPrice = 16.829999999999998,
                            ProductQuantity = 23
                        },
                        new
                        {
                            ProductID = 5,
                            ProductDescription = "Keep your smartphone handsfree with this large assembly that attaches to your rear window wiper.",
                            ProductName = "Smartphone Car Holder",
                            ProductPrice = 97.019999999999996,
                            ProductQuantity = 43
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
