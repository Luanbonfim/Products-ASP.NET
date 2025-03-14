using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;

namespace Products.Infrastructure.Persistence
{
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .HasColumnType("TEXT"); //  to addapt to SQLite

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("REAL"); //  to addapt to SQLite

            // Configure Id as auto-generated
            modelBuilder.Entity<Product>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            // Seed data without specifying Ids
            modelBuilder.Entity<Product>().HasData(
               new Product("Laptop", 999.99M),
               new Product("Smartphone", 499.99M),
               new Product("Headphones", 79.99M)
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}