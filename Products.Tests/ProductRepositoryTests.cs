using Microsoft.EntityFrameworkCore;
using Products.Application.Products.Interfaces;
using Products.Domain.Entities;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Repositories;
using Products.Tests;
using Xunit;

namespace Products.Test
{
    public class ProductRepositoryTests
    {
        private readonly DbContextOptions<ProductsDbContext> _options;
        public ProductRepositoryTests()
        {
            _options = Helper.GetInMemoryOptions<ProductsDbContext>("TestDatabase");

            SeedInitialData();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsProducts()
        {
            // Arrange


            // Act
            using (var context = new ProductsDbContext(_options))
            {
                var repository = new ProductRepository(context);
                var products = await repository.GetAllAsync();

                // Assert
                Assert.True(products.Count() > 0);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProductById()
        {
            //Arrange
            int id = 1;

            //Act 
            using (var context = new ProductsDbContext(_options))
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

            //Act 
            using (var context = new ProductsDbContext(_options))
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
            var product = new Product();
            //Act
            using (var context = new ProductsDbContext(_options))
            {
                product = context.Products.First();
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

            //Act
            using (var context = new ProductsDbContext(_options))
            {
                var productToDelete = context.Products.First();

                var repository = new ProductRepository(context);
                await repository.DeleteAsync(productToDelete.Id);

                var isDeletedProductFound = context.Products.Any(p => p.Id == productToDelete.Id);

                // Assert
                Assert.False(isDeletedProductFound);
            }
        }

        private async void SeedInitialData()
        {

            var productsList = new List<Product>{
                                                new Product { Id = 1, Name = "Test Product", Price = 100 },
                                                new Product { Id = 2, Name = "Test Product 2", Price = 100 }
                                            };

            using (var context = new ProductsDbContext(_options))
            {
                context.Products.AddRange(productsList);
                await context.SaveChangesAsync();
            }
        }
    }
}