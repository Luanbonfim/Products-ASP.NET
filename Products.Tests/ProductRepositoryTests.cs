using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Repositories;
using System.Xml.Linq;
using Xunit;

namespace Products.Test
{
    public class ProductRepositoryTests
    {
        private DbContextOptions<ProductsDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ProductsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        public ProductRepositoryTests()
        {
            SeedInitialData();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsProducts()
        {
            // Arrange: create the context and seed data
            DbContextOptions<ProductsDbContext> options = GetInMemoryOptions();

            // Act: use a new context to simulate a separate usage scenario
            using (var context = new ProductsDbContext(options))
            {
                var repository = new ProductRepository(context);
                var products = await repository.GetAllAsync();

                // Assert: verify results
                Assert.True(products.Count() > 0);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProductById()
        {
            //Arrange
            int id = 1;
            var options = GetInMemoryOptions();

            //Act 
            using (var context = new ProductsDbContext(options))
            {
                var repository = new ProductRepository(context);
                var product = await repository.GetByIdAsync(id);

                // Assert: 
                Assert.NotNull(product);
            }

        }

        [Fact]
        public async Task AddAsync_AddsAProduc()
        {
            //Arrange
            var newProduct = new Product { Id = 3, Name = "Test Product 3", Price = 100 };
            var options = GetInMemoryOptions();

            //Act 
            using (var context = new ProductsDbContext(options))
            {
                var repository = new ProductRepository(context);
                await repository.AddAsync(newProduct);

                var addedProduct = context.Products.First(p => p.Id == newProduct.Id);

                // Assert: 
                Assert.NotNull(addedProduct);
            }
        }

        [Fact]
        public async Task UpdateAsync_UpdatesAnExisistingProduct()
        {
            //Arrange 
            var options = GetInMemoryOptions();

            //Act
            using (var context = new ProductsDbContext(options))
            {
                var product = context.Products.First();
                product.Id = 1;
                product.Name = "Updated Product";
                product.Price = 150;

                var repository = new ProductRepository(context);
                await repository.UpdateAsync(product);

                var updatedProduct = context.Products.First(p => p.Id == product.Id);

                // Assert: 
                Assert.NotNull(updatedProduct);
            }
        }

        [Fact]
        public async Task DeleteAsync_DeletesAnExistingProductById()
        {
            //Arrange 
            var options = GetInMemoryOptions(); 

            //Act
            using (var context = new ProductsDbContext(options))
            {
                //context.Add(productToDelete);
                var productToDelete = context.Products.First();

                var repository = new ProductRepository(context);
                await repository.DeleteAsync(productToDelete.Id);

                var isDeletedProductFound = context.Products.Any(p => p.Id == productToDelete.Id);

                // Assert: 
                Assert.False(isDeletedProductFound);
            }
        }

        private async Task<DbContextOptions<ProductsDbContext>> SeedInitialData()
        {
            var options = GetInMemoryOptions();

            var productsList = new List<Product>{
                                                new Product { Id = 1, Name = "Test Product", Price = 100 },
                                                new Product { Id = 2, Name = "Test Product 2", Price = 100 }
                                            };

            using (var context = new ProductsDbContext(options))
            {
                context.Products.AddRange(productsList);
                await context.SaveChangesAsync();
            }

            return options;
        }
    }
}