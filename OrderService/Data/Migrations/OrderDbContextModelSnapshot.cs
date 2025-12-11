using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrderService.Data;

namespace OrderService.Data.Migrations;

[DbContext(typeof(OrderDbContext))]
// Snapshot: captures current model so EF can diff future changes into migrations.
partial class OrderDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("OrderService.Data.Entities.Order", b =>
        {
            b.Property<string>("OrderId")
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Currency")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("CustomerAddress")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("CustomerCity")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("CustomerCountry")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("CustomerEmail")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<string>("CustomerId")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<string>("CustomerName")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<string>("CustomerPostalCode")
                .IsRequired()
                .HasColumnType("text");

            b.Property<decimal>("Discount")
                .HasColumnType("numeric(18,2)");

            b.Property<DateTime>("EstimatedDeliveryDate")
                .HasColumnType("timestamp with time zone");

            b.Property<int>("ItemsNum")
                .HasColumnType("integer");

            b.Property<bool>("Paid")
                .HasColumnType("boolean");

            b.Property<string>("PaymentMethod")
                .IsRequired()
                .HasColumnType("text");

            b.Property<decimal>("ShippingCost")
                .HasColumnType("numeric(18,2)");

            b.Property<string>("ShippingAddress")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("ShippingCity")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("ShippingCountry")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("ShippingMethod")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("ShippingPostalCode")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("Source")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("character varying(50)");

            b.Property<decimal>("SubTotal")
                .HasColumnType("numeric(18,2)");

            b.Property<decimal>("Tax")
                .HasColumnType("numeric(18,2)");

            b.Property<decimal>("TotalAmount")
                .HasColumnType("numeric(18,2)");

            b.Property<string>("TransactionId")
                .IsRequired()
                .HasColumnType("text");

            b.Property<int>("Version")
                .HasColumnType("integer");

            b.HasKey("OrderId");

            b.HasIndex("CreatedAt");

            b.HasIndex("Status");

            b.ToTable("Orders");
        });

        modelBuilder.Entity("OrderService.Data.Entities.OrderItem", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<string>("Category")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("Currency")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("LineId")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("character varying(50)");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<string>("OrderId")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("character varying(100)");

            b.Property<int>("Quantity")
                .HasColumnType("integer");

            b.Property<decimal>("UnitPrice")
                .HasColumnType("numeric(18,2)");

            b.HasKey("Id");

            b.HasIndex("OrderId");

            b.HasIndex("OrderId", "LineId")
                .IsUnique();

            b.ToTable("OrderItems");
        });

        modelBuilder.Entity("OrderService.Data.Entities.OrderItem", b =>
        {
            b.HasOne("OrderService.Data.Entities.Order", null)
                .WithMany("Items")
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("OrderService.Data.Entities.Order", b =>
        {
            b.Navigation("Items");
        });
#pragma warning restore 612, 618
    }
}
