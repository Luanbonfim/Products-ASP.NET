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

            modelBuilder.Entity<Product>().HasData(
               new Product { Id = 1, Name = "Laptop", Price = 999.99M },
               new Product { Id = 2, Name = "Smartphone", Price = 499.99M },
               new Product { Id = 3, Name = "Headphones", Price = 79.99M }
           );


            base.OnModelCreating(modelBuilder);
        }

    }
}