using Microsoft.EntityFrameworkCore;
using OrderService.Data.Entities;

namespace OrderService.Data;

/*
 * DbContext is the main class in Entity Framework Core.
 * 
 * Think of it as:
 * - A "bridge" between your C# code and the database
 * - A "unit of work" - it tracks changes to entities
 * - A "repository" - it provides access to your data
 * 
 * When you inherit from DbContext, you get:
 * - Connection management (opens/closes connections automatically)
 * - Change tracking (knows what changed, what's new, what's deleted)
 * - Query translation (converts LINQ to SQL)
 * - Transaction support
 */

public class OrderDbContext : DbContext
{
    /*
     * DbSet<T> represents a table in the database.
     * 
     * DbSet<Order> Orders = the "Orders" table
     * DbSet<OrderItem> OrderItems = the "OrderItems" table
     * 
     * You can query these like collections:
     * - context.Orders.ToList() → gets all orders
     * - context.Orders.Find(id) → finds order by ID
     * - context.Orders.Add(order) → adds new order
     */
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    /*
     * Constructor - receives DbContextOptions
     * The options contain the connection string and other settings
     * This is injected by dependency injection in Program.cs
     */
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    /*
     * OnModelCreating - This method is called when EF Core builds the database model
     * 
     * We use it to:
     * - Configure relationships (foreign keys)
     * - Set up indexes
     * - Define constraints
     * - Map properties to columns
     * 
     * Fluent API - A way to configure entities using method chaining
     */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            // Set primary key
            entity.HasKey(e => e.OrderId);

            // Configure column types and constraints
            entity.Property(e => e.OrderId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.CustomerId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.CustomerName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(200)
                .IsRequired();

            // Configure decimal precision for money fields
            entity.Property(e => e.SubTotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.Tax)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.Discount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.ShippingCost)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Create index on Status for faster queries
            entity.HasIndex(e => e.Status);

            // Create index on CreatedAt for date range queries
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure the OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            // Set primary key (auto-incrementing ID)
            entity.HasKey(e => e.Id);

            // Configure foreign key relationship
            // OrderItem belongs to Order (many-to-one)
            entity.HasOne<Order>()
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // If order is deleted, delete items too

            // Configure column types
            entity.Property(e => e.OrderId)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.LineId)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Create composite index: OrderId + LineId (ensures unique line items per order)
            entity.HasIndex(e => new { e.OrderId, e.LineId })
                .IsUnique();

            // Create index on OrderId for faster lookups
            entity.HasIndex(e => e.OrderId);
        });
    }
}
