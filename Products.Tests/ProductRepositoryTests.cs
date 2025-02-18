using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Infrastructure.Messaging;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Repositories;
using Products.Tests;
using Xunit;

namespace Products.Test
{
    public class ProductRepositoryTests
    {
        private readonly DbContextOptions<ProductsDbContext> _options;
        private readonly IProductRepository _repository;
        private const string USERS_DATABASE_TEST = "TestDatabase";
        public ProductRepositoryTests()
        {
            _options = Helper.GetInMemoryOptions<ProductsDbContext>(USERS_DATABASE_TEST);

            var serviceProvider = SetupTestServiceProvider();
            _repository = serviceProvider.GetRequiredService<IProductRepository>();

            SeedInitialData();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsProducts()
        {
            // Arrange


            // Act
            var products = await _repository.GetAllAsync();

            // Assert
            Assert.True(products.Count() > 0);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProductById()
        {
            //Arrange
            int id = 1;

            //Act 
            var product = await _repository.GetByIdAsync(id);

            // Assert: 
            Assert.NotNull(product);
        }

        [Fact]
        public async Task AddAsync_AddsAProduc()
        {
            //Arrange
            var newProduct = new Product { Id = 3, Name = "Test Product 3", Price = 100 };

            //Act 
            using (var context = new ProductsDbContext(_options))
            {
                await _repository.AddAsync(newProduct);

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

                await _repository.UpdateAsync(product);

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

                await _repository.DeleteAsync(productToDelete.Id);

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

        private ServiceProvider SetupTestServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ProductsDbContext>(options => options.UseInMemoryDatabase(USERS_DATABASE_TEST));

            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddScoped<IProductRepository, ProductRepository>();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }
    }
}