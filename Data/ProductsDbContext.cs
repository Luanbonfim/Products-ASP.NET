using ProductsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductsAPI.Data
{
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .HasColumnType("TEXT"); // ✅ Fix: Use TEXT instead of nvarchar(max)

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("REAL"); // ✅ Fix: Use REAL instead of decimal(18,2)

            modelBuilder.Entity<Product>().HasData(
               new Product { Id = 1, Name = "Laptop", Price = 999.99M },
               new Product { Id = 2, Name = "Smartphone", Price = 499.99M },
               new Product { Id = 3, Name = "Headphones", Price = 79.99M }
           );


            base.OnModelCreating(modelBuilder);
        }

    }
}