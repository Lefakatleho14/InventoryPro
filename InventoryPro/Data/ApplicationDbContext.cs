using InventoryPro.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Data
{
    /// <summary>
    /// Entity Framework Core database context — the bridge between
    /// the C# models and the SQL Server database tables
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets = database tables
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Users table configuration ──────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.UserID);
                entity.Property(u => u.UserID).UseIdentityColumn(1, 1);
                entity.HasIndex(u => u.Username).IsUnique(); // username must be unique
                entity.Property(u => u.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // ── Products table configuration ───────────────────────────
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.ProductID);
                // Identity starts at 101, increments by 1
                entity.Property(p => p.ProductID).UseIdentityColumn(101, 1);
                entity.Property(p => p.QuantityInStock).HasDefaultValue(0);
                entity.Property(p => p.ReorderLevel).HasDefaultValue(5);
                entity.Property(p => p.LastUpdated).HasDefaultValueSql("GETDATE()");
                // Ensure price > 0 at DB level
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_UnitPrice", "[UnitPrice] > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_Quantity", "[QuantityInStock] >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_ReorderLevel", "[ReorderLevel] >= 0"));
            });

            // ── Sales table configuration ──────────────────────────────
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("Sales");
                entity.HasKey(s => s.SaleID);
                // Identity starts at 1001, increments by 1
                entity.Property(s => s.SaleID).UseIdentityColumn(1001, 1);
                entity.Property(s => s.SaleDate).HasDefaultValueSql("GETDATE()");
                entity.ToTable(t => t.HasCheckConstraint("CK_Sales_QuantitySold", "[QuantitySold] > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Sales_TotalPrice", "[TotalPrice] > 0"));

                // Foreign key: Sale → Product
                entity.HasOne(s => s.Product)
                      .WithMany(p => p.Sales)
                      .HasForeignKey(s => s.ProductID)
                      .OnDelete(DeleteBehavior.Restrict); // prevent cascade delete

                // Foreign key: Sale → User
                entity.HasOne(s => s.User)
                      .WithMany(u => u.Sales)
                      .HasForeignKey(s => s.UserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Seed Data ──────────────────────────────────────────────
            // Seed Users (passwords are BCrypt hashed)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Username = "lefa_m",
                    // Plain text: Manager@123
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                    Role = "Manager",
                    FullName = "Lefa Mokoena",
                    CreatedDate = new DateTime(2025, 1, 1)
                },
                new User
                {
                    UserID = 2,
                    Username = "kago_k",
                    // Plain text: Shop123
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Shop123"),
                    Role = "Shop Assistant",
                    FullName = "Kago Khumalo",
                    CreatedDate = new DateTime(2025, 1, 1)
                }
            );

            // Seed Products (IDs start at 101)
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductID = 101,
                    ProductName = "A4 Paper (500 sheets)",
                    Description = "White A4 printing paper, 80gsm",
                    QuantityInStock = 150,
                    UnitPrice = 45.00m,
                    ReorderLevel = 20,
                    Supplier = "PaperWorld Durban",
                    LastUpdated = new DateTime(2025, 1, 1)
                },
                new Product
                {
                    ProductID = 102,
                    ProductName = "Ballpoint Pens (Box of 10)",
                    Description = "Blue ballpoint pens, medium tip",
                    QuantityInStock = 8,
                    UnitPrice = 25.00m,
                    ReorderLevel = 10,
                    Supplier = "Stationery Hub",
                    LastUpdated = new DateTime(2025, 1, 1)
                },
                new Product
                {
                    ProductID = 103,
                    ProductName = "Filing Cabinet (4-drawer)",
                    Description = "Steel 4-drawer filing cabinet, grey",
                    QuantityInStock = 3,
                    UnitPrice = 850.00m,
                    ReorderLevel = 5,
                    Supplier = "Office Essentials SA",
                    LastUpdated = new DateTime(2025, 1, 1)
                },
                new Product
                {
                    ProductID = 104,
                    ProductName = "Stapler",
                    Description = "Heavy-duty stapler with staples",
                    QuantityInStock = 25,
                    UnitPrice = 55.00m,
                    ReorderLevel = 8,
                    Supplier = "Stationery Hub",
                    LastUpdated = new DateTime(2025, 1, 1)
                },
                new Product
                {
                    ProductID = 105,
                    ProductName = "Whiteboard Markers (Set of 4)",
                    Description = "Assorted colours: red, blue, green, black",
                    QuantityInStock = 4,
                    UnitPrice = 35.00m,
                    ReorderLevel = 10,
                    Supplier = "PaperWorld Durban",
                    LastUpdated = new DateTime(2025, 1, 1)
                }
            );

            // Seed Sales (IDs start at 1001)
            modelBuilder.Entity<Sale>().HasData(
                new Sale
                {
                    SaleID = 1001,
                    SaleDate = new DateTime(2025, 1, 10, 9, 30, 0),
                    ProductID = 101,
                    QuantitySold = 5,
                    TotalPrice = 225.00m,
                    UserID = 2
                },
                new Sale
                {
                    SaleID = 1002,
                    SaleDate = new DateTime(2025, 1, 10, 11, 0, 0),
                    ProductID = 102,
                    QuantitySold = 3,
                    TotalPrice = 75.00m,
                    UserID = 2
                },
                new Sale
                {
                    SaleID = 1003,
                    SaleDate = new DateTime(2025, 1, 11, 14, 0, 0),
                    ProductID = 103,
                    QuantitySold = 1,
                    TotalPrice = 850.00m,
                    UserID = 1
                }
            );
        }
    }
}